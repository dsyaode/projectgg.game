using UnityEngine;
using System.Collections;

public static class constant {

	//门的朝向
	public enum Direction
	{
		west = 1,
		east = 2,
		north = 3,
		south = 4,
	}

	//战斗类别
	public enum BattleType
	{
		Player = 1,
		Enemy = 2,
		Other = 3,
	}

	//房间类型
	public enum RoomType
	{
		Start = 1,
		Monster = 2,
		Item = 3,
	}

	public static string TAG_ENEMY = "Enemy";
	public static string TAG_PLAYER = "Player";
	public static string TAG_BULLET = "Bullet";
	public static string TAG_WALL = "Wall";
	public static string TAG_ITEM = "Item";
	public static string TAG_BASEDOORS = "BaseDoors";
        public static string TAG_NORMALDOORS = "normalDoors";
	public static string TAG_SHOPTABLE = "ShopTable";
	public static string TAG_TRAP = "Trap";
	public static string TAG_SHIT = "Shit";
        public static string TAG_STONE = "Stone";
        public static string TAG_WATER = "Water";
        public static string TAG_LIGHT = "Light";

        public static string TAG_ANIMATEDSPRITE = "AnimatedSprite";

        public static string TAG_CAMERA = "MainCamera";

        public static string TAG_FLOORLBPOINT = "FloorLBPoint";
        public static string TAG_FLOORRTPOINT = "FloorRTPoint";

	public static Direction getOppsiteDir(Direction dir){
		switch (dir) {
		case Direction.east:
			return Direction.west;
		case Direction.west:
			return Direction.east;
		case Direction.south:
			return Direction.north;
		case Direction.north:
			return Direction.south;
		default:
			Debug.Log("eror dir:" + dir);
			return Direction.east;
		}
	}

	/*
	public static bool isEqualEnum(constant.Direction dir1, constant.Direction dir2){
		return (int)dir1 == (int)dir2;
	}

	public static bool isEqualEnum(constant.RoomType dir1, constant.RoomType dir2){
		return (int)dir1 == (int)dir2;
	}
	*/

	public static GameObject getGameLogicObj(){
		return GameObject.FindGameObjectWithTag("GameLogic");
	}

	public static gamelogic getGameLogic(){
		GameObject obj = constant.getGameLogicObj ();
		return obj.GetComponent<gamelogic>();
	}

		public static soundLogic getSoundLogic(){
				return soundLogic.getInstance ();
		}

	public static maplogic getMapLogic(){
		//return GameObject.FindGameObjectWithTag("GameLogic").GetComponent<maplogic>();
		return maplogic.getInstance ();
	}

	public static uilogic getUiLogic(){
		return GameObject.FindGameObjectWithTag("GameLogic").GetComponent<uilogic>();
	}

	public static GameObject getMainCamera(){
		return GameObject.FindGameObjectWithTag("MainCamera");
	}

	public static GameObject getPlayer(){
		return GameObject.FindGameObjectWithTag("Player");
	}

	public static GameObject getLeftDownPoint(){
		return GameObject.FindGameObjectWithTag("LeftDownPoint");
	}

	public static GameObject getRightUpPoint(){
		return GameObject.FindGameObjectWithTag("RightUpPoint");
	}

        public static GameObject getCamera() {
                return GameObject.FindGameObjectWithTag(TAG_CAMERA);
        }

	public static mapfactory getMapFactory(){
		return mapfactory.getInstance ();
	}

	public static itemfactory getItemFactory(){
		return itemfactory.getInstance ();
	}

	public static monsterfactory getMonsterFactory(){
		return monsterfactory.getInstance ();
	}

	public static BattleType getBattleType(GameObject obj){
		if (obj.tag.Equals (constant.TAG_PLAYER)) {
			return BattleType.Player;
		}
		if (obj.tag.Equals (constant.TAG_ENEMY)) {
			return BattleType.Enemy;
		}
		return BattleType.Other;
	}

	public static bool isConflict(GameObject obj1, GameObject obj2){
		base_property pro1 = obj1.GetComponent<base_property> ();
		base_property pro2 = obj2.GetComponent<base_property> ();
		return isConflict (pro1, pro2);
	}

	private static bool isConflict(base_property pro1, base_property pro2){
		return pro1.isConflict (pro2);
	}
			
	public static GameObject getChildGameObject(GameObject parent, string childObjName){
		Transform[] allChildren = parent.GetComponentsInChildren<Transform>();
		foreach (Transform t in allChildren) {
			if(t.gameObject.name.Equals(childObjName)){
				return t.gameObject;
			}
			if(!t.gameObject.Equals(parent)){
				GameObject obj = getChildGameObject(t.gameObject, childObjName);
				if(obj != null){
					return obj;
				}
			}
		}
		return null;
	}

        public static GameObject getBaseParentGameObject(GameObject obj){
                if(obj.transform.parent != null){
                        return getBaseParentGameObject(obj.transform.parent.gameObject);
                }
                return obj;
        }

        public static Rect getFloorRect() {
                GameObject floorLBObj = GameObject.FindGameObjectWithTag(TAG_FLOORLBPOINT);
                GameObject floorRTObj = GameObject.FindGameObjectWithTag(TAG_FLOORRTPOINT);

                float l = floorLBObj.transform.position.x;
                float t = floorLBObj.transform.position.y;
                float w = floorRTObj.transform.position.x - l;
                float h = floorRTObj.transform.position.y - t;
                return new Rect(l,t,w,h);
        }

        public static Vector2 getMiddlePos() {
                Rect rect = getFloorRect();
                return rect.center;
        }

        public static Vector2 getRandomPosInFloor() {
                Rect rect = getFloorRect();
                float x = Random.Range(rect.xMin, rect.xMax);
                float y = Random.Range(rect.yMin, rect.yMax);
                return new Vector2(x,y);
        }

        public static Light getLight() {
                GameObject light = GameObject.FindGameObjectWithTag(TAG_LIGHT);
                return light.GetComponent<Light>();
        }
}
