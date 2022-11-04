using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderGradient : BaseMeshEffect
{
    public Color leftColor;
    public Color rightColor;
    private List<UIVertex> vertexs = new List<UIVertex>();
    private float leftX = 0;
    private float rightX = 0;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        var count = vh.currentVertCount;
        if (count == 0)
            return;
        if (vertexs.Count < 1)
        {
            for (var i = 0; i < count; i++)
            {
                var vertex = new UIVertex();
                vh.PopulateUIVertex(ref vertex, i);
                vertexs.Add(vertex);
            }

            leftX = vertexs[0].position.x;
            rightX = vertexs[0].position.x;

            for (var i = 1; i < count; i++)
            {
                var x = vertexs[i].position.x;
                if (x > rightX)
                {
                    rightX = x;
                }
                else if (x < leftX)
                {
                    leftX = x;
                }
            }
        }

        var length = rightX - leftX;
        for (var i = 0; i < count; i++)
        {
            var vertex = vertexs[i];
            var color = Color.Lerp(leftColor, rightColor, (vertex.position.x - leftX) / length);
            vertex.color = color;
            vh.SetUIVertex(vertex, i);
        }
    }
    public void SetColor(Color left, Color right)
    {
        leftColor = left;
        rightColor = right;
        // 触发setVerticesDirty 覆盖vertex的color
        graphic.SetVerticesDirty();
    }
}