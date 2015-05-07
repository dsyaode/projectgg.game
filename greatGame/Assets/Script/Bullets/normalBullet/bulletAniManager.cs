using UnityEngine;
using System.Collections;

public class bulletAniManager : MonoBehaviour {


		tk2dSprite bulletSprite;				//子弹的精灵图	
		tk2dSpriteAnimator bulletAni;			//子弹动画

		Direction crossDirection;

		public Direction BulletDirection { get { return crossDirection; } set { crossDirection = value; }}

		public bool bulletDie;

		bullet_property bulletProperty;
		// Use this for initialization

		public bool getShadow;
		public bool dynamicShadow;
		tk2dSprite shadowSprite;

                public bool noChangeDir;
                public float scaleY;
                public float scaleX;

		void Awake(){
				if (bulletDie == false) {
						bulletSprite = transform.FindChild ("ui").FindChild ("bulletPic").GetComponent<tk2dSprite> ();
						bulletAni = transform.FindChild ("ui").FindChild ("bulletPic").GetComponent<tk2dSpriteAnimator> ();
						bulletProperty = gameObject.GetComponent<bullet_property> ();
						if (getShadow) {
								shadowSprite = intiShadow ();
						}
				} else {
						GameObject.Destroy (this.gameObject);
				}

		}
		void Start () {
                        if (!noChangeDir) {
                                setBulletDirection();
                        }
		}

		// Update is called once per frame
		void Update () {

		}

		void setBulletDirection(){
				Vector3 axisX = new Vector3 (1, 0, 0);
				Vector3 bulletDirection = getDirection ();
				crossDirection = getCrossDirection (bulletDirection);
				Vector3 angle = new Vector3(0,0,angle_360 (new Vector3 (1, 0, 0), bulletDirection));
				//Debug.Log ("子弹和X轴角度: " + angle);
				bulletSprite.transform.Rotate (angle);
				if (getShadow) {
						shadowSprite.scale = new Vector3 (bulletSprite.scale.x * 0.9f, bulletSprite.scale.y * 0.7f, bulletSprite.scale.z);

                                                if (scaleY != 0) {
                                                        shadowSprite.scale = new Vector3(shadowSprite.scale.x, bulletSprite.scale.y * scaleY, bulletSprite.scale.z);
                                                }
                                                if (scaleX != 0) {
                                                        shadowSprite.scale = new Vector3(bulletSprite.scale.x * scaleX, shadowSprite.scale.y, bulletSprite.scale.z);
                                                }

                                                shadowSprite.GetComponent<shadowAniManager> ().shadowRotate (angle);

				}
				if (bulletDie == false) {
						flying ();
				}

		}

		Direction getCrossDirection(Vector3 bulletDirection){
				
				if (Mathf.Abs (bulletDirection.x) >= Mathf.Abs (bulletDirection.y)) {
						if (bulletDirection.x >= 0) {
								return Direction.left;
						} else {
								return Direction.right;
						}
				} else {
						if (bulletDirection.y >= 0) {
								return Direction.up;
						} else {
								return Direction.down;
						}
						
				}
		}

		Vector3 getDirection(){
				Vector3 bulletSpeed = rigidbody.velocity;
				return bulletSpeed.normalized;
		}
				

		float angle_360(Vector3 from_, Vector3 to_){ 
				Vector3 v3 = Vector3.Cross(from_,to_); 
				if(v3.z > 0) return Vector3.Angle(from_,to_); 
				else return 360-Vector3.Angle(from_,to_); 
		}

		//生成子弹的影子
		tk2dSprite intiShadow(){
				GameObject shadow = constant.getMapLogic ().initBulletShadow (bulletSprite , bulletSprite.transform.parent.gameObject , dynamicShadow);
				shadow.transform.localPosition = new Vector3 (0, -0.55f, 1);
				tk2dSprite shadowSprite = shadow.GetComponent<tk2dSprite> ();

                                if (scaleY != 0) {
                                        shadowSprite.scale = new Vector3(shadowSprite.scale.x, bulletSprite.scale.y * scaleY, bulletSprite.scale.z);
                                }
                                if (scaleX != 0) {
                                        shadowSprite.scale = new Vector3(bulletSprite.scale.x * scaleX, shadowSprite.scale.y, bulletSprite.scale.z);
                                }
				return shadowSprite;
		}


		//子弹击中墙壁
		public void hitWall() {
				bulletStop();
				//Debug.Log ("collision: bullet hit wall.");
				destroyAfterAni("hit");
		}

		//子弹击中敌人
		public void hitEnemies() {

				//动画结束后子弹消失
				//Debug.Log ("bulletAnimation: bullet hit enemy.");
				if (checkBulletPierce ()) {
						bulletStop();
						destroyAfterAni ("hit");
				}
		}

		//子弹达到最远距离
		public void distanceEnd() {
				bulletDie = true;
				destroyAfterAni("end");
		}

		//子弹飞行过程
		public void flying()
		{
				bulletSprite.SetSprite(bulletSprite.GetSpriteIdByName("flying"));
				bulletAni.Play("flying");
		}



		public void afterAni(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip){
				GameObject.Destroy(this.gameObject);
		}

		public void destroyAfterAni(string aniName){
				if (bulletAni) {
						bulletAni.Play (aniName);
						bulletAni.AnimationCompleted = afterAni;
				} else {
						Debug.Log ("子弹动画系统还没生成");
				}
		}

		void bulletStop(){
				bulletDie = true;
				rigidbody.velocity = new Vector3(0,0,0);
		}

		bool checkBulletPierce(){
				GameObject bullet = this.gameObject;
				bullet_property bulletPro =  bullet.GetComponent<bullet_property> ();
				bool canPierce;
				if (bulletPro) {
						//Debug.Log("bulletPro.bulletSpe.pierceBullet " + bulletPro.bulletSpe.pierceBullet);
						canPierce = bulletPro.bulletSpe.pierceBullet;
				} else {
						return true;
				}

				//如果子弹有穿透属性，则返回false 不做动画处理
				return !canPierce;

		}

		void OnTriggerEnter(Collider other){
				if(!bulletDie){
						if(other.gameObject.layer == 8){
								hitWall();
								return ;
						}
						if(other.gameObject.tag == "Wall"){
								hitWall();
								return ;
						}
						if(other.gameObject.tag == "Door"){
								hitWall();
								return ;
						}
                                                if (other.gameObject.tag == constant.TAG_NORMALDOORS) {
                                                        hitWall();
                                                        return;
                                                }

						if (bulletProperty.BattleType == constant.BattleType.Player) {
								
								enemy_property enemyPro = other.gameObject.GetComponent<enemy_property> ();
								if (enemyPro) {
										//Debug.Log("子弹击中怪物.");
										hitEnemies ();
								}
						}
						if (bulletProperty.BattleType == constant.BattleType.Enemy) {
								char_property charPro = other.gameObject.GetComponent<char_property> ();
								if (charPro) {
										//Debug.Log ("子弹击中玩家.");
										hitEnemies ();
										 
								}
						}

				}
		}
}
