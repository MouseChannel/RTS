using System.Collections;
using System.Collections.Generic;
using FixedMath;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

public partial class KDTreeSystem
{
    public NativeList<ObstacleVertice> obstacleVertices_ = new NativeList<ObstacleVertice>(Allocator.Persistent);
    public NativeList<ObstacleVerticeTreeNode> obstacleVerticesTree_ = new NativeList<ObstacleVerticeTreeNode>(Allocator.Persistent);

    public static ObstacleVerticeTreeNode obstacleVerticesTreeRoot;



    private void ObstacleVerticeCollect(NativeList<ObstacleVertice> obstacleVertices_, DynamicBuffer<PreObstacleVertice> preObstacleVertices)
    {
        if (preObstacleVertices.Length < 2)
        {
            return;
        }

        int obstacleVerticeNo = obstacleVertices_.Length;
        for (int i = 0; i < preObstacleVertices.Length; ++i)
        {
            obstacleVertices_.Add(new ObstacleVertice
            {
                verticeId_ = obstacleVertices_.Length,
                previous_ = -1,
                next_ = -1,
                obstacleId_ = obstacleVerticeNo,

            });
        }

        for (int i = 0; i < preObstacleVertices.Length; ++i)
        {

            ObstacleVertice obstacleVertice = obstacleVertices_[obstacleVertices_.Length - preObstacleVertices.Length + i];
            obstacleVertice.point_ = preObstacleVertices[i].vertice;

            if (i != 0)
            {

                obstacleVertice.previous_ = obstacleVertice.verticeId_ - 1;

                var temp = obstacleVertices_[obstacleVertice.previous_];
                temp.next_ = obstacleVertice.verticeId_;
                obstacleVertices_[obstacleVertice.previous_] = temp;

            }

            if (i == preObstacleVertices.Length - 1)
            {
                obstacleVertice.next_ = obstacleVertices_[obstacleVerticeNo].verticeId_;

                var temp = obstacleVertices_[obstacleVertice.next_];
                temp.previous_ = obstacleVertice.verticeId_;
                obstacleVertices_[obstacleVertice.next_] = temp;
                // obstacle.next_.previous_ = obstacle;
            }

            obstacleVertice.direction_ = FixedCalculate.Normalize(preObstacleVertices[(i == preObstacleVertices.Length - 1 ? 0 : i + 1)].vertice - preObstacleVertices[i].vertice);

            if (preObstacleVertices.Length == 2)
            {
                obstacleVertice.convex_ = true;
            }
            else
            {
                obstacleVertice.convex_ = (FixedCalculate.LeftOf(preObstacleVertices[(i == 0 ? preObstacleVertices.Length - 1 : i - 1)].vertice, preObstacleVertices[i].vertice, preObstacleVertices[(i == preObstacleVertices.Length - 1 ? 0 : i + 1)].vertice) >= 0);
            }
            obstacleVertices_[obstacleVertice.verticeId_] = obstacleVertice;


        }


    }

    private void InitObstacleVerticeTree(int length)
    {
        for (int i = 0; i < length; i++)
        {
            obstacleVerticesTree_.Add(new ObstacleVerticeTreeNode
            {
                obstacleVertice_Index = i,
                left_index = -1,
                right_index = -1
            });
        }
    }

    [BurstCompile]
    private struct BuildObstacleVerticeTreeJob : IJob
    {
        public NativeList<ObstacleVertice> obstacleVertices_;
        public NativeList<ObstacleVerticeTreeNode> obstacleVerticesTree_;

        // public ObstacleVerticeTreeNode obstacleVerticesTreeRoot;
        public void Execute()
        {
            


            NativeList<ObstacleVertice> currentObstacleVertices = new NativeList<ObstacleVertice>(Allocator.Temp);
            currentObstacleVertices.AddRange(obstacleVertices_);
            obstacleVerticesTreeRoot = BuildObstacleVerticeTreeRecursive(currentObstacleVertices);

            
            currentObstacleVertices.Dispose();

        }

        private ObstacleVerticeTreeNode BuildObstacleVerticeTreeRecursive(NativeList<ObstacleVertice> current)
        {
            if (current.Length == 0) return new ObstacleVerticeTreeNode { obstacleVertice_Index = -1 };

            ObstacleVerticeTreeNode node = new ObstacleVerticeTreeNode();
            int length = current.Length;

            int optimalSplit = 0;
            int minLeft = current.Length; ;
            int minRight = current.Length;

            for (int i = 0; i < current.Length; ++i)
            {
                int leftSize = 0;
                int rightSize = 0;

                ObstacleVertice obstacleI1 = current[i];
                ObstacleVertice obstacleI2 = obstacleVertices_[obstacleI1.next_];

                /* Compute optimal split node. */
                for (int j = 0; j < current.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    ObstacleVertice obstacleJ1 = current[j];



                    ObstacleVertice obstacleJ2 = obstacleVertices_[obstacleJ1.next_];

                    FixedInt j1LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    FixedInt j2LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


                    if (j1LeftOfI >= -FixedCalculate.superSmallValue && j2LeftOfI >= -FixedCalculate.superSmallValue)
                    {
                        ++leftSize;
                    }
                    else if (j1LeftOfI <= FixedCalculate.superSmallValue && j2LeftOfI <= FixedCalculate.superSmallValue)
                    {
                        ++rightSize;
                    }
                    else
                    {
                        ++leftSize;
                        ++rightSize;
                    }

                    if (new FixedIntPair(FixedCalculate.Max(leftSize, rightSize), FixedCalculate.Min(leftSize, rightSize)) >= new FixedIntPair(FixedCalculate.Max(minLeft, minRight), FixedCalculate.Min(minLeft, minRight)))
                    {
                        break;
                    }
                }


                if (new FixedIntPair(FixedCalculate.Max(leftSize, rightSize), FixedCalculate.Min(leftSize, rightSize)) < new FixedIntPair(FixedCalculate.Max(minLeft, minRight), FixedCalculate.Min(minLeft, minRight)))
                {
                    minLeft = leftSize;
                    minRight = rightSize;

                    optimalSplit = i;
                }
            }

            {
                /* Build split node. */
                NativeList<ObstacleVertice> leftObstacles = new NativeList<ObstacleVertice>(Allocator.Temp);
                // IList<Obstacle> leftObstacles = new List<Obstacle>(minLeft);

                for (int n = 0; n < minLeft; ++n)
                {
                    leftObstacles.Add(new ObstacleVertice { verticeId_ = -1 });
                }

                NativeList<ObstacleVertice> rightObstacles = new NativeList<ObstacleVertice>(Allocator.Temp);
                // IList<Obstacle> rightObstacles = new List<Obstacle>(minRight);

                for (int n = 0; n < minRight; ++n)
                {
                    rightObstacles.Add(new ObstacleVertice { verticeId_ = -1 });
                }

                int leftCounter = 0;
                int rightCounter = 0;
                int i = optimalSplit;

                ObstacleVertice obstacleI1 = current[i];
                ObstacleVertice obstacleI2 = obstacleVertices_[obstacleI1.next_];

                for (int j = 0; j < current.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    ObstacleVertice obstacleJ1 = current[j];
                    ObstacleVertice obstacleJ2 = obstacleVertices_[obstacleJ1.next_];

                    FixedInt j1LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                    FixedInt j2LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


                    if (j1LeftOfI >= -FixedCalculate.superSmallValue && j2LeftOfI >= -FixedCalculate.superSmallValue)
                    {
                        leftObstacles[leftCounter++] = current[j];
                    }
                    else if (j1LeftOfI <= FixedCalculate.superSmallValue && j2LeftOfI <= FixedCalculate.superSmallValue)
                    {
                        rightObstacles[rightCounter++] = current[j];
                    }
                    else
                    {
                        /* Split obstacle j. */
                        FixedInt t = FixedCalculate.Det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleI1.point_) / FixedCalculate.Det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleJ2.point_);

                        FixedVector2 splitPoint = obstacleJ1.point_ + t * (obstacleJ2.point_ - obstacleJ1.point_);

                        ObstacleVertice newObstacle = new ObstacleVertice();
                        newObstacle.point_ = splitPoint;
                        newObstacle.previous_ = obstacleJ1.verticeId_;
                        newObstacle.next_ = obstacleJ2.verticeId_;
                        newObstacle.convex_ = true;
                        newObstacle.direction_ = obstacleJ1.direction_;

                        newObstacle.verticeId_ = obstacleVertices_.Length;


                        // Simulator.Instance.obstacles_.Add(newObstacle);
                        obstacleVertices_.Add(newObstacle);
                        obstacleVerticesTree_.Add(new ObstacleVerticeTreeNode());

                        obstacleJ1.next_ = newObstacle.verticeId_;
                        obstacleJ2.previous_ = newObstacle.verticeId_;

                        if (j1LeftOfI > 0)
                        {
                            leftObstacles[leftCounter++] = obstacleJ1;
                            rightObstacles[rightCounter++] = newObstacle;
                        }
                        else
                        {
                            rightObstacles[rightCounter++] = obstacleJ1;
                            leftObstacles[leftCounter++] = newObstacle;
                        }

                    }


                }

                node.obstacleVertice_Index = obstacleI1.verticeId_;

                node.left_index = BuildObstacleVerticeTreeRecursive(leftObstacles).obstacleVertice_Index;
                node.right_index = BuildObstacleVerticeTreeRecursive(rightObstacles).obstacleVertice_Index;
                obstacleVerticesTree_[node.obstacleVertice_Index] = node;
                leftObstacles.Dispose();
                rightObstacles.Dispose();

                return node;
            }
        }



    }

    private ObstacleVerticeTreeNode BuildObstacleVerticeTreeRecursive(NativeList<ObstacleVertice> current)
    {
        if (current.Length == 0) return new ObstacleVerticeTreeNode { obstacleVertice_Index = -1 };

        ObstacleVerticeTreeNode node = new ObstacleVerticeTreeNode();
        int length = current.Length;

        int optimalSplit = 0;
        int minLeft = current.Length; ;
        int minRight = current.Length;

        for (int i = 0; i < current.Length; ++i)
        {
            int leftSize = 0;
            int rightSize = 0;

            ObstacleVertice obstacleI1 = current[i];
            ObstacleVertice obstacleI2 = obstacleVertices_[obstacleI1.next_];

            /* Compute optimal split node. */
            for (int j = 0; j < current.Length; ++j)
            {
                if (i == j)
                {
                    continue;
                }

                ObstacleVertice obstacleJ1 = current[j];



                ObstacleVertice obstacleJ2 = obstacleVertices_[obstacleJ1.next_];

                FixedInt j1LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                FixedInt j2LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


                if (j1LeftOfI >= -FixedCalculate.superSmallValue && j2LeftOfI >= -FixedCalculate.superSmallValue)
                {
                    ++leftSize;
                }
                else if (j1LeftOfI <= FixedCalculate.superSmallValue && j2LeftOfI <= FixedCalculate.superSmallValue)
                {
                    ++rightSize;
                }
                else
                {
                    ++leftSize;
                    ++rightSize;
                }

                if (new FixedIntPair(FixedCalculate.Max(leftSize, rightSize), FixedCalculate.Min(leftSize, rightSize)) >= new FixedIntPair(FixedCalculate.Max(minLeft, minRight), FixedCalculate.Min(minLeft, minRight)))
                {
                    break;
                }
            }


            if (new FixedIntPair(FixedCalculate.Max(leftSize, rightSize), FixedCalculate.Min(leftSize, rightSize)) < new FixedIntPair(FixedCalculate.Max(minLeft, minRight), FixedCalculate.Min(minLeft, minRight)))
            {
                minLeft = leftSize;
                minRight = rightSize;

                optimalSplit = i;
            }
        }

        {
            /* Build split node. */
            NativeList<ObstacleVertice> leftObstacles = new NativeList<ObstacleVertice>(Allocator.Temp);
            // IList<Obstacle> leftObstacles = new List<Obstacle>(minLeft);

            for (int n = 0; n < minLeft; ++n)
            {
                leftObstacles.Add(new ObstacleVertice { verticeId_ = -1 });
            }

            NativeList<ObstacleVertice> rightObstacles = new NativeList<ObstacleVertice>(Allocator.Temp);
            // IList<Obstacle> rightObstacles = new List<Obstacle>(minRight);

            for (int n = 0; n < minRight; ++n)
            {
                rightObstacles.Add(new ObstacleVertice { verticeId_ = -1 });
            }

            int leftCounter = 0;
            int rightCounter = 0;
            int i = optimalSplit;

            ObstacleVertice obstacleI1 = current[i];
            ObstacleVertice obstacleI2 = obstacleVertices_[obstacleI1.next_];

            for (int j = 0; j < current.Length; ++j)
            {
                if (i == j)
                {
                    continue;
                }

                ObstacleVertice obstacleJ1 = current[j];
                ObstacleVertice obstacleJ2 = obstacleVertices_[obstacleJ1.next_];

                FixedInt j1LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ1.point_);
                FixedInt j2LeftOfI = FixedCalculate.LeftOf(obstacleI1.point_, obstacleI2.point_, obstacleJ2.point_);


                if (j1LeftOfI >= -FixedCalculate.superSmallValue && j2LeftOfI >= -FixedCalculate.superSmallValue)
                {
                    leftObstacles[leftCounter++] = current[j];
                }
                else if (j1LeftOfI <= FixedCalculate.superSmallValue && j2LeftOfI <= FixedCalculate.superSmallValue)
                {
                    rightObstacles[rightCounter++] = current[j];
                }
                else
                {
                    /* Split obstacle j. */
                    FixedInt t = FixedCalculate.Det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleI1.point_) / FixedCalculate.Det(obstacleI2.point_ - obstacleI1.point_, obstacleJ1.point_ - obstacleJ2.point_);

                    FixedVector2 splitPoint = obstacleJ1.point_ + t * (obstacleJ2.point_ - obstacleJ1.point_);

                    ObstacleVertice newObstacle = new ObstacleVertice();
                    newObstacle.point_ = splitPoint;
                    newObstacle.previous_ = obstacleJ1.verticeId_;
                    newObstacle.next_ = obstacleJ2.verticeId_;
                    newObstacle.convex_ = true;
                    newObstacle.direction_ = obstacleJ1.direction_;

                    newObstacle.verticeId_ = obstacleVertices_.Length;


                    // Simulator.Instance.obstacles_.Add(newObstacle);
                    obstacleVertices_.Add(newObstacle);
                    obstacleVerticesTree_.Add(new ObstacleVerticeTreeNode());

                    obstacleJ1.next_ = newObstacle.verticeId_;
                    obstacleJ2.previous_ = newObstacle.verticeId_;

                    if (j1LeftOfI > 0)
                    {
                        leftObstacles[leftCounter++] = obstacleJ1;
                        rightObstacles[rightCounter++] = newObstacle;
                    }
                    else
                    {
                        rightObstacles[rightCounter++] = obstacleJ1;
                        leftObstacles[leftCounter++] = newObstacle;
                    }

                }


            }

            node.obstacleVertice_Index = obstacleI1.verticeId_;

            node.left_index = BuildObstacleVerticeTreeRecursive(leftObstacles).obstacleVertice_Index;
            node.right_index = BuildObstacleVerticeTreeRecursive(rightObstacles).obstacleVertice_Index;
            obstacleVerticesTree_[node.obstacleVertice_Index] = node;
            leftObstacles.Dispose();
            rightObstacles.Dispose();

            return node;
        }
    }




}
