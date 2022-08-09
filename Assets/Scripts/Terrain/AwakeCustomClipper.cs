using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vector2i = ClipperLib.IntPoint;

public class AwakeCustomClipper : MonoBehaviour, IClip
{
    public DestructibleTerrain terrain;

    public bool CheckBlockOverlapping(Vector2 p, float size)
    {       
        float dx = 0/*Mathf.Abs(clipPosition.x - p.x) - radius - size / 2*/;
        float dy = 0/*Mathf.Abs(clipPosition.y - p.y) - radius - size / 2*/;

        return dx < 0f && dy < 0f;      
    }

    public ClipBounds GetBounds()
    {   
        float colCenter = 20;
        Vector2 clipPosition = GetPositionCustomClip();

        return new ClipBounds
        {
            lowerPoint = new Vector2(clipPosition.x - colCenter, clipPosition.y - colCenter),
            upperPoint = new Vector2(clipPosition.x + colCenter, clipPosition.y + colCenter)
        };             
    }

    public List<Vector2i> GetVertices()
    {
        List<Vector2i> vertices = new List<Vector2i>();

        PolygonCollider2D clipperCol = GetComponent<PolygonCollider2D>();
        if(clipperCol != null)
        {
        Vector2[] colliderPoints = clipperCol.points;
        Vector2 clipPosition = GetPositionCustomClip();
        
        for(int i = 0; i < colliderPoints.Length; i++)
        {
            Vector2 point = new Vector2(colliderPoints[i].x + clipPosition.x, colliderPoints[i].y + clipPosition.y);
            Vector2i point_i64 = point.ToVector2i();
            vertices.Add(point_i64);            
        }
        clipperCol.enabled = false;
        }
        return vertices;
        
    }

    Vector2 GetPositionCustomClip()
    {
        Vector2 positionWorldSpace = transform.position;      
        Vector2 clipPosition = positionWorldSpace - terrain.GetPositionOffset();

        return clipPosition;
    }
    void Start()
    {
        terrain.ExecuteCustomClip(this);
    }
}
