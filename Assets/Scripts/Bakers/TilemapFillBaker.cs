using System;
using System.Collections.Generic;
using System.Numerics;
using Clipper2Lib;
using ASK.Helpers;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using World;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Bakers
{
    public class TilemapFillBaker : MonoBehaviour, IBaker
    {
        [Tooltip("Which tilemap to fill up")]
        [SerializeField] private Tilemap fillMap;
        [Tooltip("Which tile to use")]
        [SerializeField] private TileBase fillTile;

        [Tooltip("Position to start floodfilling")]
        [SerializeField] private Vector2Int fillPoint;

        [FormerlySerializedAs("offset")]
        [Tooltip("How much space between rooms and tiles. Should be 0.5*camera size")]
        [SerializeField] private int padding;
        
        [Tooltip("Add this offset to every point")]
        [SerializeField] private Vector2 pointsOffset;
        [SerializeField] private Vector2Int pointsMargin;

        [SerializeField] private Vector2[] innerPoints;
        [SerializeField] private Vector2[] outerPoints;
        
        public void ClearTiles()
        {
            fillMap.ClearAllTiles();
        }

        public void Bake()
        {
            CalculatePoints();
            DrawLines();
            Fill();
            /*foreach (var p in OffsetPath(ret))
            {
                var col = transform.GetChild(0).gameObject.AddComponent<EdgeCollider2D>();
                col.points = PathToPoints(p);
            }*/
        }

        public void CalculatePoints()
        {
            Room[] rooms = FindObjectsOfType<Room>();
            Paths64 ret = new Paths64();
            var initPaths = PointsToPath(
                rooms[0].GetComponent<PolygonCollider2D>().points,
                rooms[0].transform.position
            );
            
            ret.Add(Clipper.MakePath(initPaths));

            foreach (var room in rooms)
            {
                var roomPts = room.GetComponent<PolygonCollider2D>().points;
                ret = CombinePoints(ret, PointsToPath(roomPts, room.transform.position));
            }
            ret[0].Add(ret[0][0]);

            Paths64 translatedPath = Clipper.TranslatePaths(ret, pointsMargin.x, pointsMargin.y);
            ret = Clipper.Union(ret, translatedPath, FillRule.NonZero);

            var offsetRet = OffsetPath(ret);

            innerPoints = PathToPoints(ret[0]);
            outerPoints = PathToPoints(offsetRet[0]);
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            #endif
        }

        public void ClearPoints()
        {
            innerPoints = null;
            outerPoints = null;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
            #endif
        }

        public void DrawLines()
        {
            ClearTiles();
            DrawTilePoints(innerPoints);
            DrawTilePoints(outerPoints);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(fillMap);
            #endif
        }

        public void Fill()
        {
            if (innerPoints == null || outerPoints == null) return;
            
            Path64 innerPath = Clipper.MakePath(PointsToPath(innerPoints, Vector2.zero));
            Path64 outerPath = Clipper.MakePath(PointsToPath(outerPoints, Vector2.zero));
            Point64 fillStartPos = new Point64(fillPoint.x, fillPoint.y);
            bool fillOutsideInner = Clipper.PointInPolygon(fillStartPos, innerPath) == PointInPolygonResult.IsOutside;
            bool fillInsideOuter = Clipper.PointInPolygon(fillStartPos, outerPath) == PointInPolygonResult.IsInside;
            if (fillInsideOuter && fillOutsideInner)
            {
                Vector3Int fillStartPosV = new Vector3Int((int)fillStartPos.X, (int)fillStartPos.Y);
                Vector3Int tilePos = fillMap.WorldToCell(fillStartPosV);
                fillMap.FloodFill(tilePos, fillTile);
            }
            else
            {
                Debug.LogError("Tilemap fill point must be between inner path and outer path");
            }
        }

        private void DrawTilePoints(Vector2[] points)
        {
            for(int i = 0; i < points.Length; ++i)
            {
                Vector2 p0 = points[i];
                Vector2 p1 = i + 1 >= points.Length ? points[0] : points[i+1];
                var tilep0 = fillMap.WorldToCell(p0);
                var tilep1 = fillMap.WorldToCell(p1);

                var line = LineGenerator(
                    new Vector2Int(tilep0.x, tilep0.y),
                    new Vector2Int(tilep1.x, tilep1.y)
                );
                fillMap.SetTiles(line.tilePts, line.tileObjs);
            }
        }

        private (Vector3Int[] tilePts, TileBase[] tileObjs) LineGenerator(Vector2Int p0, Vector2Int p1)
        {
            List<Vector3Int> tilePts = new();
            List<TileBase> tiles = new();

            Vector2Int diff = p1 - p0;
            int len = 0;

            if (diff.x == 0)
            {
                len = Math.Abs(diff.y);
            }
            else
            {
                len = Math.Abs(diff.x);
            }

            Vector2Int direction = new Vector2Int(Math.Sign(diff.x), Math.Sign(diff.y));
            for (int i = 0; i < len; ++i)
            {
                var add = p0 + direction * i;
                tilePts.Add(new Vector3Int(add.x, add.y));
                tiles.Add(fillTile);
            }

            return (tilePts: tilePts.ToArray(), tileObjs: tiles.ToArray());
        }

        private TileBase GetBaseTile(Tilemap tmap, Vector3 offset)
        {
            return tmap.GetTile(tmap.WorldToCell(offset));
        }

        private Paths64 OffsetPath(Paths64 subj)
        {
            return Clipper.InflatePaths(subj, padding, JoinType.Miter, EndType.Polygon, 100);
        }

        private Paths64 CombinePoints(Paths64 subj, int[] p1)
        {
            Paths64 clip = new Paths64();
            clip.Add(Clipper.MakePath(p1));
            return Clipper.Union(subj, clip, FillRule.NonZero);
        }
        
        private int[] PointsToPath(Vector2[] p1, Vector2 offset) {
            List<int> path1 = new();
            foreach (var v in p1)
            {
                var tempV = v;
                tempV += offset;
                path1.Add((int)Math.Round(tempV.x));
                path1.Add((int)Math.Round(tempV.y));
            }
            return path1.ToArray();
        }
        
        private Vector2[] PathToPoints(Path64 p) {
            List<Vector2> ret = new();
            foreach (Point64 i in p)
            {
                ret.Add(new Vector2(i.X, i.Y) + pointsOffset);
            }
            return ret.ToArray();
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.red;
            DrawPoints(innerPoints);
            DrawPoints(outerPoints);
            Gizmos.DrawSphere(fillPoint.ToVector2(), 16);
        }

        private static void DrawPoints(Vector2[] pts)
        {
            if (pts == null) return;
            Vector3[] pts3 = pts.ToVector3();
            Handles.DrawPolyLine(pts3);
            Handles.DrawLine(pts3[^1], pts3[0]);
        }
        #endif
    }
}