using System;
using System.Collections.Generic;
using System.Threading;
using HLODSystem.Extensions;
using UnityEngine;

namespace HLODSystem.SpaceManager
{
    public class QuadTreeSpaceSplitter
    {

        enum SplitAxis
        {
            X,
            Y,
            Z
        }

        private List<SpaceNode> spaceNodes = new List<SpaceNode>();
        private Vector3 chunkSize = Vector3.one;
        public Vector3 spaceMinSize = Vector3.one;

        public List<SpaceNode> SpaceNodes => spaceNodes;
        
        public SpaceNode GetSpaceNode(int index)
        {
            if (index >= spaceNodes.Count)
            {
                return null;
            }
            else
            {
                return spaceNodes[index];
            }
        }

        public SpaceNodeData[] SerializeSpaceNode()
        {
            List<SpaceNodeData> spaceNodeData = new List<SpaceNodeData>();
            foreach (var tmpSpaceNode in spaceNodes)
            {
                if (!tmpSpaceNode.IsEmpty)
                {
                    spaceNodeData.Add(tmpSpaceNode.GetData());
                }
            }

            return spaceNodeData.ToArray();
        }



        public void AddSpaceNode(SpaceNode spaceNode)
        {
            spaceNode.index = spaceNodes.Count;
            spaceNodes.Add(spaceNode);
        }

        public SpaceNode CreateSpaceTree(ref Bounds initBounds, Vector3 size)
        {
            chunkSize = size;
            SpaceNode rootNode = new SpaceNode
            {
                Bounds = initBounds
            };
            var nodeStack = new Stack<SpaceNode>();
            nodeStack.Push(rootNode);
            AddSpaceNode(rootNode);
            while (nodeStack.Count > 0)
            {
                var node = nodeStack.Pop();
                List<SpaceNode> childNodes = null;
                if (node.Bounds.size.x > chunkSize.x && node.Bounds.size.y > chunkSize.y && node.Bounds.size.z > chunkSize.z)
                {
                    // 3轴超过限制 八叉树
                    childNodes = SplitSpaceNodeEight(node);
                } else if (node.Bounds.size.x > chunkSize.x && node.Bounds.size.y > chunkSize.y)
                {
                    // 2轴超过限制 四叉树 XY平面 Z轴
                    childNodes = SplitSpaceFour(node, SplitAxis.Z);
                } else if (node.Bounds.size.x > chunkSize.x && node.Bounds.size.z > chunkSize.z)
                {
                    // 2轴超过限制 四叉树 XY平面 Y轴
                    childNodes = SplitSpaceFour(node, SplitAxis.Y);
                } else if (node.Bounds.size.y > chunkSize.y && node.Bounds.size.z > chunkSize.z)
                {
                    // 2轴超过限制 四叉树 XY平面 X轴
                    childNodes = SplitSpaceFour(node, SplitAxis.X);
                } else if (node.Bounds.size.x > chunkSize.x)
                {
                    // 1轴超过限制 二叉树  X轴
                    childNodes = SplitSpaceTwo(node, SplitAxis.X);
                }
                else if (node.Bounds.size.y > chunkSize.y)
                {
                    // 1轴超过限制 二叉树  Y轴
                    childNodes = SplitSpaceTwo(node, SplitAxis.Y);
                }
                else if (node.Bounds.size.z > chunkSize.z)
                {
                    // 1轴超过限制 二叉树  X轴
                    childNodes = SplitSpaceTwo(node, SplitAxis.Z);
                }

                if (childNodes != null)
                {
                    for (int i = 0; i < childNodes.Count; ++i)
                    {
                        AddSpaceNode(childNodes[i]);
                        childNodes[i].ParentNode = node;
                        nodeStack.Push(childNodes[i]);
                    }
                }
                else
                {
                    spaceMinSize = node.Bounds.size;
                }
            }
            LoggerUtils.Log("spaceNodes:" + spaceNodes.Count);
            return rootNode;
        }


        
        private List<SpaceNode> SplitSpaceTwo(SpaceNode parentNode, SplitAxis axis)
        {
            List<SpaceNode> childSpaceNodes = new List<SpaceNode>(2);
            var boundsSize = parentNode.Bounds.size;
            Vector3 center = parentNode.Bounds.center;
            Vector3 offset = Vector3.zero;
            Vector3 childBoundsSize = boundsSize;
            switch (axis)
            {
                case SplitAxis.X:
                    offset.x = boundsSize.x * 0.25f;
                    childBoundsSize.x *= 0.5f;
                    break;
                case SplitAxis.Y:
                    offset.y = boundsSize.y * 0.25f;
                    childBoundsSize.y *= 0.5f;
                    break;
                case SplitAxis.Z:
                    offset.z = boundsSize.z * 0.25f;
                    childBoundsSize.z *= 0.5f;
                    break;
            }
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center - offset, childBoundsSize)));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + offset, childBoundsSize)));
            return childSpaceNodes;
        }
        
        private List<SpaceNode> SplitSpaceFour(SpaceNode parentNode, SplitAxis axis)
        {
            List<SpaceNode> childSpaceNodes = new List<SpaceNode>(4);
            var boundsSize = parentNode.Bounds.size;
            Vector3 center = parentNode.Bounds.center;
            Vector3 offset = Vector3.zero;
            Vector3 childBoundsSize = boundsSize;
            switch (axis)
            {
                 case SplitAxis.X:
                     offset.y = boundsSize.y * 0.25f;
                     offset.z = boundsSize.z * 0.25f;
                     childBoundsSize.y *= 0.5f;
                     childBoundsSize.z *= 0.5f;
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(0, offset.y, offset.z), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(0, -offset.y, offset.z), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(0, offset.y, -offset.z), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(0, -offset.y, -offset.z), childBoundsSize)));
                     break;
                 case SplitAxis.Y:
                     offset.x = boundsSize.x * 0.25f;
                     offset.z = boundsSize.z * 0.25f;
                     childBoundsSize.x *= 0.5f;
                     childBoundsSize.z *= 0.5f;
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(offset.x, 0, offset.z), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(-offset.x, 0, offset.z), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(offset.x, 0, -offset.z), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(-offset.x, 0, -offset.z), childBoundsSize)));
                     break;
                 case SplitAxis.Z:
                     offset.y = boundsSize.y * 0.25f;
                     offset.x = boundsSize.x * 0.25f;
                     childBoundsSize.y *= 0.5f;
                     childBoundsSize.x *= 0.5f;
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(offset.x, offset.y, 0), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(-offset.x, -offset.y, 0), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(offset.x, offset.y, 0), childBoundsSize)));
                     childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(center + new Vector3(-offset.x, -offset.y, 0), childBoundsSize)));
                     break;
            }
            return childSpaceNodes;
        }

        private List<SpaceNode> SplitSpaceNodeEight(SpaceNode parentNode)
        {
            List<SpaceNode> childSpaceNodes = new List<SpaceNode>(8);
            var boundsSize = parentNode.Bounds.size;
            Vector3 center = parentNode.Bounds.center;
            Vector3 offset = boundsSize / 4.0f;
            Vector3 childBoundsSize = boundsSize / 2.0f;
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(-offset.x, -offset.y, -offset.z), childBoundsSize
                )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(offset.x, -offset.y, -offset.z), childBoundsSize
            )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(-offset.x, -offset.y, offset.z), childBoundsSize
            )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(offset.x, -offset.y, offset.z), childBoundsSize
            )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(-offset.x, offset.y, -offset.z), childBoundsSize
            )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(offset.x, offset.y, -offset.z), childBoundsSize
            )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(-offset.x, offset.y, offset.z), childBoundsSize
            )));
            childSpaceNodes.Add(SpaceNode.CreateSpaceNodeWithBounds(new Bounds(
                center + new Vector3(offset.x, offset.y, offset.z), childBoundsSize
            )));
            return childSpaceNodes;
        }

    }
}