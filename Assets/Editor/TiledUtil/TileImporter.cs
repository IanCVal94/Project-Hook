using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Clipper2Lib;

using SuperTiled2Unity;
using SuperTiled2Unity.Editor;

using ASK.Helpers;
using Helpers;
using MyBox;
using Spawning;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

using World;
using Vector2 = UnityEngine.Vector2;
using LIL = TiledUtil.LayerImportLibrary;

namespace TiledUtil {

    [AutoCustomTmxImporter()]
    public class TileImporter : CustomTmxImporter, IFilterLoggerTarget {
        private PrefabReplacementsData _prefabReplacements;

        private const string CUSTOM_PREFIX = "C_";

        public override void TmxAssetImported(TmxAssetImportedArgs data)
        {
            _prefabReplacements = LoadPrefabReplacements(data);
            SuperMap map = data.ImportedSuperMap;     
            
            //Don't import automap Tilesets
            if (map.gameObject.name.StartsWith("automap")) return;
            
            var args = data.AssetImporter;
            var layers = map.GetComponentsInChildren<SuperLayer>();
            var superObjects = map.GetComponentsInChildren<SuperObject>();
            var doc = XDocument.Load(args.assetPath);
            
            AddRoomComponents(map.transform);

            //Applies to the entire tilemap
            Dictionary<String, Action<GameObject>> tilemapLayerImports = new()
            {
                //{ "Lava", ImportLavaTilemap },
                { "Ground", ImportGroundTilemap },
                { "Semisolids", ImportSemisolidsTilemap },
                { "Water", ImportWaterTilemap },
                { "Dirt", ImportGroundTilemap },
                { "Windows", ImportWindowsTilemap },
                //{ "DecorBack", ImportDecorBackTilemap },
                //{ "GlowingMushrooms", ImportGlowingMushroomTilemap },
                { "Stalagtites", ImportStalagtitesTilemap },
                { "Spikes", ImportSpikesTilemap },
                { "Branches", ImportBranchesTilemap },
                //{ "Doors", ImportDoorsTilemap },
                //{ "Vines", ImportVinesTilemap },
            };
            
            //Applies to children
            Dictionary<String, Action<GameObject, int>> tileLayerImports = new() {
                { "Ground", ImportGround },
                { "Semisolids", ImportSemisolids },
                { "Dirt", ImportGround },
                { "Windows", ImportWindows },
                //{ "Breakable", ImportBreakable },
                //{ "GlowingMushrooms", ImportGlowingMushroom },
                // { "Stalagtites", ImportStalagtites },
                //{ "Lava", ImportLava },
                //{ "Water", ImportWater },
                //{ "Doors", ImportDoors },
            };

            Dictionary<String, Action<GameObject>> tilemapLayerImportsLate = new()
            {
                { "Ground", ImportGroundLayerLate }
            };
            
            foreach (SuperLayer layer in layers) {
                string layerName = layer.name;
                
                if (tilemapLayerImports.ContainsKey(layerName))
                {
                    tilemapLayerImports[layerName](layer.gameObject);
                }

                if (tileLayerImports.ContainsKey(layerName)) {
                  
                    ResolveTileLayerImports(layer.transform, tileLayerImports[layerName]);
                }

                if (tilemapLayerImportsLate.ContainsKey(layerName))
                    tilemapLayerImportsLate[layerName](layer.gameObject);
            }
        }
        
        private struct PrefabReplacementsData
        {
            public Dictionary<string, GameObject> standard;
            public Dictionary<string, GameObject> custom;
        }

        private PrefabReplacementsData LoadPrefabReplacements(TmxAssetImportedArgs data)
        {
            var typePrefabReplacements = data.AssetImporter.SuperImportContext.Settings.PrefabReplacements;
            var wholeDict = typePrefabReplacements.ToDictionary(so => so.m_TypeName, so => so.m_Prefab);
            return SeparatePrefabReplacements(wholeDict);
        }
        
        private PrefabReplacementsData SeparatePrefabReplacements(Dictionary<string, GameObject> dict)
        {
            var data = dict.GroupBy(kv => kv.Key.StartsWith(CUSTOM_PREFIX)).ToList();
            PrefabReplacementsData ret = new PrefabReplacementsData();
            ret.standard = data[0].CollapseToDictionary();
            if (data.Count > 0) ret.custom = data[1].CollapseToDictionary();
            return ret;
        }

        private void ImportWindowsTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().enabled = false;
        }
        
        private void ImportWindows(GameObject g, int index)
        {
            var points = g.GetRequiredComponent<EdgeCollider2D>().points;
            Vector2 scale;
            String prefabName;
            if (Mathf.Abs(points[0].y - points[1].y) < 9)
            {
                scale = new Vector2(1, 4);
                prefabName = "Window Long";
            }
            else
            {
                scale = new Vector2(4, 1);
                prefabName = "Window Tall";
            }
            
            var data = LIL.TileToPrefab(g, index, _prefabReplacements.standard[prefabName]);
            
            g = data.gameObject;
            Vector2[] colliderPoints = data.collisionPts;
            g.GetRequiredComponent<BoxCollider2D>().offset = Vector2.zero;
            
            Vector2[] spritePoints = LIL.ColliderPointsToRectanglePoints(g, colliderPoints);
            
            Vector2 avgSpritePoint = spritePoints.ComputeAverage();
            g.transform.localPosition = avgSpritePoint;
            
            for (int i = 0; i < spritePoints.Length; ++i) spritePoints[i].Scale(scale);
            
            LIL.SetNineSliceSprite(g, spritePoints);
            LIL.SetLayer(g, "Ground");
            g.GetRequiredComponent<SpriteRenderer>().SetSortingLayer("Main");
        }

        private void AddRoomComponents(Transform room)
        {
            room.gameObject.AddComponent<Room>();
            room.gameObject.AddComponent<RoomSpawnSolver>();

            Tilemap mainTilemap = FindGroundLayerTilemap(room);
            if (mainTilemap == null)
            {
                FilterLogger.LogWarning(this,   $"Room bounds and components not added to tiled map {room.gameObject.name} " +
                                                $"because it does not contain a Tiled layer named 'Ground' or 'Dirt'.");
                return;
            }
            mainTilemap.CompressBounds();
            PolygonCollider2D bounds = AddPolygonColliderToRoom(room, mainTilemap);
            // AddVCamManagerToRoom(room, bounds);
        }

        private Tilemap FindGroundLayerTilemap(Transform parent)
        {
            SuperTileLayer[] layers = parent.GetComponentsInChildren<SuperTileLayer>();
            foreach (SuperTileLayer layer in layers)
            {
                if (layer.gameObject.name.Equals("Ground") || layer.gameObject.name.Equals("Dirt"))
                {
                    return layer.GetComponent<Tilemap>();
                }
            }

            return null;
        }

        private PolygonCollider2D AddPolygonColliderToRoom(Transform room, Tilemap mainTilemap)
        {
            // Bounds colliderBounds = mainTilemap.localBounds;

            var points = mainTilemap.GetComponentInChildren<EdgeCollider2D>().points;
            
            
            
            PolygonCollider2D roomCollider = room.gameObject.AddComponent<PolygonCollider2D>();
            roomCollider.pathCount = 0;
            roomCollider.points = points;
            /*Vector2 boundsMin = colliderBounds.min;
            Vector2 boundsMax = colliderBounds.max;
            float alpha = 0.01f;
            roomCollider.SetPath(0, new Vector2[]
            {
                boundsMin - Vector2.one * alpha,
                boundsMin + Vector2.right * colliderBounds.extents.x * 2 + new Vector2(alpha, -alpha),
                boundsMax + Vector2.one * alpha,
                boundsMin + Vector2.up * colliderBounds.extents.y * 2 + new Vector2(-alpha, alpha),
            });
            roomCollider.offset = mainTilemap.transform.position;
            roomCollider.isTrigger = true;*/

            return roomCollider;
        }

        private void AddVCamManagerToRoom(Transform room, PolygonCollider2D boundingShape)
        {
            GameObject instance = _prefabReplacements.standard["VCamManager"];
            instance = (GameObject)PrefabUtility.InstantiatePrefab(instance);
            instance.transform.SetParent(room);

            instance.GetComponent<VCamManager>().SetConfiner(boundingShape);
        }

        private GameObject AddWaterfalCollision(GameObject g, Vector2[] points)
        {
            GameObject waterfallReplace = _prefabReplacements.standard["WaterfallCollider"];
            waterfallReplace = LIL.CreatePrefab(waterfallReplace, 0, g.transform);
            LIL.SetEdgeCollider2DPoints(waterfallReplace, points);
            return waterfallReplace;
        }

        private void ImportGround(GameObject g, int index) {
            var ret = LIL.TileToPrefab(g, index, _prefabReplacements.standard["Ground"]);
            // AddWaterfalCollision(ret.gameObject, ret.collisionPts);
            LIL.SetLayer(ret.gameObject, "Ground");
        }
        
        private void ImportGlowingMushroom(GameObject g, int index)
        {
            var ret = LIL.TileToPrefab(g, index, _prefabReplacements.standard["Glowing Mushroom"]);
            ret.gameObject.transform.position = ret.collisionPts[2] + new Vector2(4, -12);
            ret.gameObject.transform.localScale = new Vector3(Mathf.Round(UnityEngine.Random.value)*2-1, 1, 1);
        }

        // private void ImportStalagtites(GameObject g, int index)
        // {
            
        // }
        
        private void ImportSemisolids(GameObject g, int index) {
            var ret = LIL.TileToPrefab(g, index, _prefabReplacements.standard["Semisolid"]);
            LIL.SetLayer(ret.gameObject, "Ground");
        }

        private void ImportBreakable(GameObject g, int index) {
            var data = LIL.TileToPrefab(g, index, _prefabReplacements.standard["Breakable"]);
            g = data.gameObject;
            Vector2[] colliderPoints = data.collisionPts;
            Vector2[] spritePoints = LIL.ColliderPointsToRectanglePoints(g, colliderPoints); 
            
            Vector2 avgSpritePoint = spritePoints.ComputeAverage();
            colliderPoints = colliderPoints.ComputeNormalized(avgSpritePoint);
            g.transform.localPosition = avgSpritePoint;
            
            LIL.SetNineSliceSprite(g, spritePoints);
            LIL.SetEdgeCollider2DPoints(g, colliderPoints);
            LIL.AddShadowCast(g, colliderPoints.ToVector3());
            LIL.SetLayer(g, "Ground");
            g.GetRequiredComponent<SpriteRenderer>().SetSortingLayer("Interactable");
            AddWaterfalCollision(g, colliderPoints);
        }

        private void ImportDoors(GameObject g, int index)
        {
            var ret = LIL.TileToPrefab(g, index, _prefabReplacements.standard["Door"]);
            LIL.SetLayer(ret.gameObject, "Default");
        }

        private void ImportLavaTilemap(GameObject g)
        {
            LIL.SetMaterial(g, "Lava");
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Lava");
        }

        private void ImportBranchesTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Above Ground Decor");
            // LayerImportLibrary.SetMaterial(g, "Mask_Graph");
        }

        private void ImportDoorsTilemap(GameObject g)
        {
            g.GetComponent<TilemapRenderer>().enabled = false;
            g.transform.parent = g.transform.parent.parent;
        }
        
        private void ImportVinesTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Vines");
        }

        private void ImportSpikesTilemap(GameObject g)
        {
            GameObject.DestroyImmediate(g);
        }
        
        private void ImportDecorBackTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Bg");
        }

        private void ImportGlowingMushroomTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().enabled = false;
        }

        private void ImportStalagtitesTilemap(GameObject g)
        {
            var r = g.GetRequiredComponent<TilemapRenderer>();
            r.SetSortingLayer("Bg");
            r.sortingOrder = 5;
        }
        
        private void ImportGroundTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Main");
        }
        
        void ImportGroundLayerLate(GameObject g)
        {
            Transform main = g.transform.GetChild(0);
            if (main.childCount < 2) return;
            
            var pCollider0 = main.GetChild(0).GetComponent<PolygonCollider2D>();
            var pCollider1 = main.GetChild(1).GetComponent<PolygonCollider2D>();
            var p0 = Clipper.PointsToPath(pCollider0.points, pCollider0.transform.position);
            var p1 = Clipper.PointsToPath(pCollider1.points, pCollider1.transform.position);
            
            var paths = new Paths64();
            paths.Add(p0);
            paths.Add(p1);

            if (Clipper.Contains(p0, p1))
            {
                pCollider0.pathCount++;
                pCollider0.SetPath(1, pCollider1.points);
                GameObject.DestroyImmediate(pCollider1.gameObject);
            }
        }
        
        private void ImportSemisolidsTilemap(GameObject g)
        {
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Main");
        }

        private void ImportWaterTilemap(GameObject g)
        {
            g.SetLayerRecursively("Water");
            g.GetRequiredComponent<TilemapRenderer>().SetSortingLayer("Water");
            LIL.SetMaterial(g, "Mask_Graph");
        }

        private void ResolveTileLayerImports(Transform layer, Action<GameObject, int> import) {
            if (layer.childCount > 0) {
                Transform t = layer.GetChild(0);
                t.ForEachChild(import);
            }
        }

        public XElement GetLayerXNode(XDocument doc, SuperLayer layer) {
            foreach (XElement xNode in doc.Element("map").Elements()) {
                XAttribute curName = xNode.Attribute("name");
                if (curName != null && curName.Value == layer.name) {
                    return xNode;
                }
            }

            return null;
        }

        public LogLevel GetLogLevel()
        {
            return LogLevel.Warning;
        }
    }
}