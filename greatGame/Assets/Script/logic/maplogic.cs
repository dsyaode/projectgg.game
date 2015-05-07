using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class maplogic{
		public bool debugLog;
		private static maplogic mInstance;

		private maplogic(){
				mPrefabStr = "Prefabs/char/robot";
				mFloorIndex = 1;
		}

		public static maplogic getInstance(){
				if (mInstance == null) {
						mInstance = new maplogic();
				}
				return mInstance;
		}

		public mapinfo mMapInfo;
		public bool isFinishScene = false;
		public GameObject mDoor;
		GameObject[] mDoorSprs;

		//public GameObject mCharPrefabs;		//主角prefab
		private string mPrefabStr = "Prefabs/char/robot";
		public GameObject mChar;

		public static bool mIsStartGame = false;

		public int mFloorIndex;

		public void Awake(){
				mPrefabStr = "Prefabs/char/robot";
				mFloorIndex = 1;
		}

		public int getFloorTotalRoomNum(){
				int num = 4 + mFloorIndex*(mFloorIndex+1)/2;
				return num;
		}

		public void resetStartGame(){
				mIsStartGame = false;
		}

		public roominfo getCurRoom(){
				if (mMapInfo == null) {
						return null;
				}
				roominfo roomInfo = mMapInfo.getCurRoom ();
				return roomInfo;
		}

		public void initCharPos(){
				roominfo roomInfo = mMapInfo.getCurRoom ();

				float x = 0;
				float y = 0;

				doorinfo doorInfo = roomInfo.getDoorInfo (roomInfo.mEnterDoorId);
				if (doorInfo != null) {
						//Debug.Log ("roomInfoEnterDoorId: " + roomInfo.mEnterDoorId + "，doorInfo:" + doorInfo.mId);
						//Debug.Log ("doorInfo.mDir:" + doorInfo.mDir);

						switch (doorInfo.mDir) {
						case constant.Direction.east:
								x = 4;
								break;
						case constant.Direction.west:
								x = -4;
								break;
						case constant.Direction.south:
								y = -4;
								break;
						case constant.Direction.north:
								y = 4;
								break;
						}

						GameObject door = getDoorTouchObj (doorInfo.mId);



						if (door != null) {
								GameObject role = GameObject.FindGameObjectWithTag (constant.TAG_PLAYER);
								role.gameObject.transform.position = new Vector3 (door.transform.position.x + x, door.transform.position.y + y, -1);
						}
				} else {
						//Debug.Log("error roomInfo.mEnterDoorId:" + roomInfo.mEnterDoorId );
				}

		}


		public float getRoomInfoFAcc(){
			if (mMapInfo != null) {
				return mMapInfo.getRoomInfoFAcc ();
			}

			//test
			return 60.0f;
		}

		public GameObject getDoorTouchObj(int doorId){
				List<string> doornames = new List<string>();
				doornames.Add ("UpRoom");
				doornames.Add ("DownRoom");
				doornames.Add ("LeftRoom");
				doornames.Add ("RightRoom");

				foreach (string name in doornames) {
						GameObject[] objs = GameObject.FindGameObjectsWithTag(name);
						foreach(GameObject obj in objs){
								room_property pro = obj.GetComponent<room_property>();
								if(pro.mDoorInfo.mId == doorId){
										return obj;
								}
						}
				}

				return null;
		}

		public void initRoomInfo(){

				Dictionary<string, constant.Direction> dic = new Dictionary<string, constant.Direction>();
				dic.Add ("UpRoom", constant.Direction.south);
				dic.Add ("DownRoom", constant.Direction.north);
				dic.Add ("LeftRoom", constant.Direction.east);
				dic.Add ("RightRoom", constant.Direction.west);

				roominfo roomInfo = mMapInfo.getCurRoom ();
				List<doorinfo> doorinfos = roomInfo.mDoorInfos;

				foreach(KeyValuePair<string, constant.Direction> pair in dic)
				{
						GameObject[] objs = GameObject.FindGameObjectsWithTag(pair.Key);
						if(objs != null){
								foreach(GameObject obj in objs){
										char_enter_script com = obj.GetComponent<char_enter_script>();
										com.mDir = pair.Value;

										//Debug.Log("obj:" + obj.name);

										room_property pro = obj.GetComponent<room_property>();
										//Debug.Log ("pro:" + pro.mX + "," + pro.mY + "," + (pro.mDoorInfo));
										foreach(doorinfo info in doorinfos){
												//Debug.Log ("info:" + info.mX + "," + info.mY);
												if(pro.mX == info.mX && pro.mY == info.mY){
														//Debug.Log("add roominfo");
														pro.mDoorInfo = info;
														//Debug.Log ("info:" + info);
														//Debug.Log ("pro.mDoorInfo:" + pro.mDoorInfo.mX);

                                                                                                                GameObject spr = obj.transform.parent.transform.Find("Sprite0").gameObject;
                                                                                                                spr.SetActive(true);	

														if(info.mNextRoomId != 0){
																GameObject wall = obj.transform.parent.transform.Find("wall").gameObject;
																wall.SetActive(false);
														}
												}

										}
								}
						}

				}

				initRoomSceneInfo (roomInfo);
		}

		//生成房间的道具或者怪物的prefabs的gameobject
		public void initRoomSceneInfo(roominfo info){
				//Debug.Log ("info.mItemPrefabs:" +info.mItemPrefabs.Count);
				//Debug.Log ("info.mMonsterPrefabs:" +info.mMonsterPrefabs.Count);
		SceneTemplate scentemplate = constant.getMapFactory ().getSceneTemplate (info.mSceneIndex);
		if (scentemplate.ScenePrefab != null) {
			GameObject clone = (GameObject)GameObject.Instantiate (Resources.Load (scentemplate.ScenePrefab), new Vector3 (0, 0, 0), Quaternion.identity);
		}

				foreach (KeyValuePair<itemtemplate, Vector3> pair in info.mItemPrefabs) {
						Vector3 v = pair.Value;
						GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load(pair.Key.PrefabPath),v,Quaternion.identity);
				}

				foreach (KeyValuePair<monstertemplate, Vector3> pair in info.mMonsterPrefabs) {
						Vector3 v = pair.Value;
						GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load(pair.Key.PrefabPath),v,Quaternion.identity);
				}
		}

		public void initMapInfo(){
				mFloorIndex = 1;
				mMapInfo = constant.getMapFactory().getRandomMap (this);

                                //播放背景音乐
                                soundLogic.getInstance().playBackGround("musicbk");
		}

		void Start () {
		}

		public void startRoom(){
				Debug.Log ("startRoom"
				);
				if (!mIsStartGame) {			
						Debug.Log ("game start");			
						startMap ();
						mIsStartGame = true;
				}
				createChar ();

				isFinishScene = true;
				mDoorSprs = GameObject.FindGameObjectsWithTag("normalDoors");
				mChar = GameObject.FindGameObjectWithTag(constant.TAG_PLAYER);

				initRoomInfo ();
				initCharPos ();
		}

		// Update is called once per frame
		public void Update () {
				/*
		if (mDoorSprs == null) {
			return;
		}

		foreach (GameObject doorSpr in mDoorSprs) {
			BoxCollider collider = doorSpr.GetComponent<BoxCollider>();
			collider.isTrigger = true;
		}
		*/

				checkOpenDoor ();
		}

		void startGame(){

		}

		void startMap(){
				initMapInfo ();
				enterFirstRoom ();
		}

		void enterFirstRoom(){
				//GameObject.DontDestroyOnLoad (this.gameObject);
				//GameObject role = GameObject.FindGameObjectWithTag (constant.TAG_PLAYER);
				//DontDestroyOnLoad (role);
				GameObject role = GameObject.FindGameObjectWithTag (constant.TAG_PLAYER);
				if (role != null) {
						GameObject.DestroyImmediate(role);
				}
				roominfo roomInfo = mMapInfo.getFirstRoom ();// mapfactory.getNextRoom (mMapInfo, doorInfo);
				loadRoom(roomInfo);
		}

		void loadRoom(roominfo info){
				//Debug.Log ("enterRoom " + info.mId);
				Application.LoadLevel(info.mSceneIndex);
		}

		void createChar(){
				Vector3 v = new Vector3 ();
				v.x = -6.820158f;
				v.y = -0.5814921f;
				v.z = -1f;

				Debug.Log ("createChar");
				GameObject role = GameObject.FindGameObjectWithTag (constant.TAG_PLAYER);
				if (role == null) {
						GameObject charClone = (GameObject)GameObject.Instantiate(Resources.Load(mPrefabStr),v,Quaternion.identity);
				}

		}

		public void enterRoom(GameObject door, GameObject role){
				doorinfo doorInfo = door.GetComponent<room_property>().mDoorInfo;
				//Debug.Log ("enterRoom doorInfo:" + doorInfo.mX + ","
				//           + doorInfo.mY + "," 
				//           + doorInfo.mDir + ","
				//           + doorInfo.mNextRoomId + ","
				//           + doorInfo.mNextDoorId );

				role.gameObject.rigidbody.velocity = new Vector3 (0,0,0);
				//role.gameObject.transform.position = new Vector3 (-6, 0, -1);

				//char_enter_script enter = door.GetComponent<char_enter_script> ();
				//Debug.Log ("enterRoom dir:" + enter.mDir);

				//GameObject.DontDestroyOnLoad (this.gameObject);
				GameObject.DontDestroyOnLoad (role);
				roominfo roomInfo = constant.getMapFactory().getNextRoom (mMapInfo, doorInfo);
				loadRoom(roomInfo);
		}

		public void openDoor(){
				GameObject[] doors = GameObject.FindGameObjectsWithTag ("normalDoors");
				roominfo info = constant.getMapLogic ().getCurRoom ();
				//Debug.Log ("openDoor");
				foreach (GameObject door in doors) {
						GameObject doorTouch = door.transform.parent.Find("door_touch").gameObject;
						room_property pro = doorTouch.GetComponent<room_property>();
						if(pro.mDoorInfo != null && pro.mDoorInfo.hasNext() && info.mEnterDoorId != pro.mDoorInfo.mId){
								tk2dSpriteAnimator ani = door.GetComponent<tk2dSpriteAnimator>();
								//Debug.Log("openDoor:" + door.name);
								BoxCollider box = ani.gameObject.GetComponent<BoxCollider> ();
								if(!box.isTrigger){
										ani.Play("open");
										ani.AnimationCompleted = playOpenDoorAni;
								}
						}
				}
		}

	public void playOpenDoorAni(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip){
		//Debug.Log ("playOpenDoorAni");
		BoxCollider box = animator.gameObject.GetComponent<BoxCollider> ();
		box.isTrigger = true;
	}
	
	public void checkOpenDoor(){
		GameObject[] enemys = GameObject.FindGameObjectsWithTag (constant.TAG_ENEMY);
		//Debug.Log ("checkOpenDoor: " +  enemys.Length);
		if (enemys.Length == 0) {
			openDoor();
		}
	}

	
		//碰撞， 不能穿过的物体， 例如人物，怪物，道具
		public void colliderEnter(GameObject collider, GameObject beCollider){
				touchEnter (collider, beCollider);
		}

		//触发，可以穿过的物体，例如子弹
		public void triggerEnter(GameObject collider, GameObject beCollider){
				touchEnter (collider, beCollider);
		}

		//接触检测OntriggerEnter
		private void touchEnter(GameObject collider, GameObject beCollider){
				string colliderTag = collider.tag;
				string beColliderTag = beCollider.tag;
				//Debug.Log ("colliderTag:" + colliderTag);
				//Debug.Log ("beColliderTag:" + beColliderTag);

				if (colliderTag.Equals (constant.TAG_ENEMY)) {
						if(beColliderTag.Equals(constant.TAG_PLAYER)){
								if(constant.isConflict(collider, beCollider)){
										attack(collider, beCollider);
								}
						}
				}

				if (colliderTag.Equals (constant.TAG_SHIT)) {
					if(beColliderTag.Equals(constant.TAG_PLAYER)){
						if(constant.isConflict(collider, beCollider)){
							attack(collider, beCollider);
						}
					}
				}

				if (colliderTag.Equals (constant.TAG_ITEM)) {

						if(beColliderTag.Equals(constant.TAG_PLAYER)){
								eatItem(beCollider, collider);
						}
				}

				if (colliderTag.Equals (constant.TAG_ENEMY)) {
						if(beColliderTag.Equals(constant.TAG_BULLET)){
								if(constant.isConflict(collider, beCollider)){
										attack(beCollider, collider);
								}
						}
				}

				if (colliderTag.Equals (constant.TAG_PLAYER)) {
						if(beColliderTag.Equals(constant.TAG_BULLET)){
								if(constant.isConflict(collider, beCollider)){
										attackByBullet(beCollider, collider);
								}
						}
				}

				if (colliderTag.Equals (constant.TAG_TRAP)) {
					if(beColliderTag.Equals(constant.TAG_PLAYER)){
						Debug.Log("trap player");
						attack(collider, beCollider);
					}
				}

                                if (colliderTag.Equals(constant.TAG_STONE)) {
                                        if (beColliderTag.Equals(constant.TAG_PLAYER)) {
                                                Debug.Log("stone player");
                                                attack(collider, beCollider);
                                        }
                                }

				if (colliderTag.Equals (constant.TAG_SHOPTABLE)) {
						if(beColliderTag.Equals(constant.TAG_PLAYER)){
								//购买道具
								buyItem(collider, beCollider);
						}
				}
						

		}
	


		//捡道具	
		private void eatItem(GameObject player, GameObject item){
				//Debug.Log ("mapLogic eatItem");
				charlogic charLogic = player.GetComponent<charlogic> ();
				if (charLogic.grapItem (item.gameObject)) {
						monsterbaselogic baseLogic = item.GetComponent<monsterbaselogic> ();
						baseLogic.destroy ();
				}
		}

		public void showDamage(){

		}


		////背包相关//////
		public void bagAddIcon(int id){

		}

		////装备相关//////
		public void playerAddEquipment(GameObject player, GameObject item){
				charlogic charLogic = player.GetComponent<charlogic> ();
				if (charLogic.equipItem (item)) {
						monsterbaselogic baseLogic = item.GetComponent<monsterbaselogic> ();
						baseLogic.destroy ();
				}

		}
		//背包增加金币
		public void bagAddGold(int amount , GameObject player){
				char_property charProperty = player.GetComponent<char_property> ();
				charProperty.addGold (amount);

		}
		//背包增加宝石
		public void bagAddDiamond(int amount , GameObject player){
				//Debug.Log ("还没有宝石");
		}

		//背包增加普通钥匙
		public void bagAddKey(int amount , GameObject player){
				char_property charProperty = player.GetComponent<char_property> ();
				charProperty.addKey (amount);
		}

		//背包增加金钥匙
		public void bagAddGoldenKey(int amount , GameObject player){
				//Debug.Log ("还没有金钥匙");

		}

		public void buyItem(GameObject collider, GameObject beCollider){
				Debug.Log ("玩家开始购买道具");
				if (beCollider.GetComponent<charlogic> ().buyItem (collider) ){
						collider.GetComponent<buyItem_Property> ().buyItem (true);
						Debug.Log ("购买成功");

				}
				else {
						collider.GetComponent<buyItem_Property> ().buyItem (false);
						Debug.Log ("购买失败");
				}
		}




		//攻击 伤害
		private void attack(GameObject atker, GameObject beAtker){
				monsterbaselogic beAtkerLogic = beAtker.GetComponent<monsterbaselogic>();
				beAtkerLogic.beAttack(atker);
		}
				
        public void shakeCamera(){
                GameObject camera = constant.getCamera();
                camera_follow_script follow = camera.GetComponent<camera_follow_script>();
                follow.shake();
        }
		private void attackByBullet(GameObject atker, GameObject beAtker){
				monsterbaselogic beAtkerLogic = beAtker.GetComponent<monsterbaselogic>();
				beAtkerLogic.beAttackByBullet(atker);
		}

		//敌人设置子弹属性
		public bulletSpeStruct setUpBulletSpeStruct(enemyShotBullet bulletProperty){
				
				bulletSpeStruct bulletSpe;
				bulletSpe.pierceBullet = bulletProperty.pierceBullet;
				bulletSpe.knockType = bulletProperty.knockType;
				bulletSpe.element = bulletProperty.enemyType;
				bulletSpe.scaredBullet.scaredEffect = bulletProperty.scaredEffect;
				bulletSpe.scaredBullet.sacredPercent = bulletProperty.scaredPercent;
				bulletSpe.slowBullet.slowEffect = bulletProperty.slowEffect;
				bulletSpe.slowBullet.slowLevel = bulletProperty.slowLevel;
				bulletSpe.slowBullet.slowPercent = bulletProperty.slowPercent;

				return bulletSpe;
		}


		//在gameobject播放一次性动画
		public void onceEffectAni(GameObject obj, string aniPath , string aniLibPath , string aniName){
				//使用播放一次性动画的prefab
				GameObject itemEffectClone = (GameObject)GameObject.Instantiate(Resources.Load(aniPath),obj.transform.position,Quaternion.identity);
				itemEffectClone.transform.parent = obj.gameObject.transform;
				itemEffectClone.transform.localPosition = new Vector3 (0, 0, -2);
				onceAniManager aniManager = itemEffectClone.GetComponent<onceAniManager> ();
				aniManager.setAniLib (aniLibPath);
				aniManager.playAni(aniName);	
		}


		//生成物体的影子
		public GameObject initBulletShadow(tk2dSprite sprite , GameObject father , bool dynamicShadow){
				string shadowPath =  "Prefabs/aniEffect/shadow_bullet";
				GameObject bulletShadowClone = (GameObject)GameObject.Instantiate(Resources.Load(shadowPath),father.transform.localPosition,Quaternion.identity);
				bulletShadowClone.transform.parent = father.transform;
				bulletShadowClone.transform.localPosition =	father.transform.localPosition;
				//bulletShadowClone.transform.localPosition = new Vector3 (obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z + 1);
				bulletShadowClone.GetComponent<shadowAniManager>().setUp(sprite , dynamicShadow);
				return bulletShadowClone;

		}

		//怪物恢复颜色时候获得状态对应的颜色
		public Color getStateColor(GameObject obj){
				enemylogic enemyLogic = obj.GetComponent<enemylogic> ();
				//获得object最优先的状态颜色
				BattleStates battleState = enemyLogic.getStates ();
				Color stateColor = StateColor.normal;

				if (battleState == BattleStates.slowDown) {
						stateColor = StateColor.slowDown;	
						return stateColor;
				}

				if (battleState == BattleStates.scared) {
						stateColor = StateColor.scared;	
						return stateColor;
				}


				if (battleState == BattleStates.normal) {
						stateColor = StateColor.normal;	
						return stateColor;
				}

				return stateColor;
		}


                public void normalShake() {
                        GameObject camera = constant.getCamera();
                        camera_follow_script follow = camera.GetComponent<camera_follow_script>();
                        follow.normalShake();
                }

        public void setPlayerCanCotroll(bool ret){
                GameObject player = constant.getPlayer();
                charlogic logic = player.GetComponent<charlogic>();
                logic.CanControll = ret;

        }

        public void setNormalLight() {
                Light light = constant.getLight();
                light.type = LightType.Directional;
                light.intensity = 0.35f;
        }

        public void pushChar(GameObject obj) {
                GameObject player = constant.getPlayer();

                Vector3 v = player.transform.position - obj.transform.position;
                float force = 2000f;

                maplogic logic = constant.getMapLogic();

                player.transform.rigidbody.AddForce(v.normalized * force);
        }
}


