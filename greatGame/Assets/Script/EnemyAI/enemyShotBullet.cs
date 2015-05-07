using UnityEngine;
using System.Collections;

public class enemyShotBullet : MonoBehaviour {

		//子弹的prefab 如果不输出为默认
		public string bulletName;
		string  bulletPath = "Prefabs/bullets/normalBullet";	//子弹prefab
		weaponType weapontype;

		bulletGetSpeed shotScript;				//子弹用script(设定速度)
		Direction bulletDirection;				//子弹的射出方向

		public Vector3 shotPos;

		//子弹的属性
		public float baseBulletRate;			//子弹origin间隔时间
		float mbulletRate;
		public float baseBulletSpeed;				//子弹的origin速度
		float mbulletSpeed;
		public float baseBulletDistance;			//子弹的origin距离
		float mbulletDistance;
		public int baseBulletDamage;				//bullet damage
		int mbulletDamage;
		//伤害频率
		public float mdamageRate;

		//子弹属性
		public ElementType enemyType;

		//子弹的击退效果
		public KnockType knockType;
		public int mknockBack;
	


		public bulletSpeStruct bulletSpe;
		//穿透
		public bool pierceBullet;
		//恐惧
		public bool scaredEffect;
		public int scaredPercent;
		//减速
		public bool slowEffect;
		public int slowPercent;
		public float slowLevel;





		float delayTime=0;	

		void Awake(){
				checkForget ();
				weapontype = weaponType.bulletNormal;
				if (bulletName != "") {
						bulletPath = "Prefabs/bullets/" + bulletName;
				}

		}

		// Use this for initialization
		void Start () {
				setUpBullet ();
		}

		// Update is called once per frame
		void Update () {

		}

		void setUpBullet(){
				bulletSpe = constant.getMapLogic ().setUpBulletSpeStruct (this);
		}

		public void shootBullet(EnemyShotType shotType)
		{
				GameObject bulletClone;
				Vector3 shotBulletPos = this.transform.position + shotPos;
				bulletClone = (GameObject)Instantiate(Resources.Load(bulletPath),shotBulletPos,Quaternion.identity);

				if (shotType == EnemyShotType.directRandom) {
						
				}

				if (shotType == EnemyShotType.directPlayer) {
						//set bullet end distance
						bulletClone.GetComponent<bulletCheckDistance> ().setDistance (mbulletDistance);
						//子弹的damage + 速度 +方向
						setBulletProperty (bulletClone);
						shotScript = bulletClone.GetComponent<bulletGetSpeed> ();
						setBulletSpeed (shotScript, mbulletSpeed);
				}
				if (shotType == EnemyShotType.random) {
						bulletClone.GetComponent<bulletCheckDistance> ().setDistance (mbulletDistance);
						//子弹的damage + 速度 +方向
						setBulletProperty (bulletClone);
						shotScript = bulletClone.GetComponent<bulletGetSpeed> ();
						setRandomSpeed (shotScript, mbulletSpeed);
				}

		}

                public GameObject shootBullet(float dir) {
                        GameObject bulletClone;
                        Vector3 shotBulletPos = this.transform.position + shotPos;
                        bulletClone = (GameObject)Instantiate(Resources.Load(bulletPath), shotBulletPos, Quaternion.identity);

                        bulletClone.GetComponent<bulletCheckDistance>().setDistance(mbulletDistance);
                        //子弹的damage + 速度 +方向
                        setBulletProperty(bulletClone);
                        //shotScript = bulletClone.GetComponent<bulletGetSpeed>();
                        //setRandomSpeed(shotScript, mbulletSpeed);
                        shotScript = bulletClone.GetComponent<bulletGetSpeed>();
                        Vector3 speedDir = new Vector3(Mathf.Cos(dir), Mathf.Sin(dir), 0);
                        shotScript.shotBullet(speedDir.normalized * mbulletSpeed);

                        return bulletClone;
                }

		public void shootMultiBullets(EnemyShotType shotType , int bulletAmount , int angle){
				//子弹是向着主角方向发散型
				if (shotType == EnemyShotType.directDiverging) {
						Vector3 playerPos = getPlayerPosExact ();
						for (int i = 1; i <= bulletAmount; i++) {
								GameObject bulletClone = (GameObject)Instantiate(Resources.Load(bulletPath),this.transform.position,Quaternion.identity);	
								bulletClone.GetComponent<bulletCheckDistance> ().setDistance (mbulletDistance);
								setBulletProperty (bulletClone);
								shotScript = bulletClone.GetComponent<bulletGetSpeed> ();
								setDirectionDivergingSpeed (shotScript, mbulletSpeed , playerPos , i , angle);
						}
				}
		}

		//获得玩家的精确位置
		Vector3 getPlayerPosExact(){
				return constant.getPlayer ().transform.position;
		}

		Vector3 getPlayerPosExcur(int amount){
				//Vector3 playerPos = GameObject.FindWithTag ("Player").transform.position;
				Vector3 playerPos = constant.getPlayer ().transform.position;
				playerPos.x += Random.Range (-amount, amount + 1)/10;
				playerPos.y += Random.Range (-amount, amount + 1)/10;
				return playerPos;
		}

		//设置子弹飞行方向
		void setDirectionDivergingSpeed(bulletGetSpeed speedScript,float Speed , Vector3 pos , int num , int angle){
				Vector3 speedDir = new Vector3(0,0,0);
				Vector3 enemyPos = this.transform.position + shotPos;
				speedDir = pos - enemyPos;
				speedDir.z = 0;
				//Debug.Log (num + " num : old Vector = " + speedDir);
				//speedDir.Normalize ();
				//Quaternion rot =  new Quaternion(0,0, Mathf.Sin(num*10/2) , Mathf.Cos(num*10/2) );
				speedScript.gameObject.transform.position += speedDir.normalized;
				Quaternion rot;
				if (num % 2 == 1) {
						num -= 1;
						rot = Quaternion.Euler (0f, 0f, -1 * (num / 2) * angle);
				} else {
						rot = Quaternion.Euler (0f, 0f, (num / 2) * angle);
				}
				speedDir = rot * speedDir;
				//Debug.Log (num + " num : new Vector = " + speedDir);
				//speedDir.Normalize ();
				//speedDir.x += (num-1)*4;
				speedScript.shotBullet(speedDir.normalized * Speed);
		}




		//设置子弹飞行方向
		void setRandomSpeed(bulletGetSpeed speedScript,float Speed){
				int randomX = Random.Range (-180, 181);
				int randomY = Random.Range (-180, 181);
				Vector3 speedDir = new Vector3(randomX,randomY,0);
				speedScript.shotBullet(speedDir.normalized * Speed);
		}

		//设置子弹飞行方向
		void setBulletSpeed(bulletGetSpeed speedScript,float Speed){
				Vector3 speedDir = new Vector3(0,0,0);
				Vector3 enemyPos = this.transform.position+shotPos;
				Vector3 playerPos = getPlayerPosExact ();
				speedDir = playerPos - enemyPos;
				speedDir.z = 0;
				speedScript.shotBullet(speedDir.normalized * Speed);
		}

		public void setBulletProperty(GameObject bulletClone){
				//如果忘记输入子弹射击频率,默认为0.5秒间隔
				if(mdamageRate == 0) {mdamageRate = 0.5f;}
				bulletClone.GetComponent<bullet_property>().setProperty(weapontype,mbulletDamage,mknockBack,mdamageRate,bulletSpe, constant.getBattleType(this.gameObject));
		}

		//enemy_logic里调用
		public void upgradeProperties(enemy_property property){
				mbulletSpeed = baseBulletSpeed + property.AttackSpeed;
				mbulletRate = baseBulletRate - (baseBulletRate-0.1f)*property.AttackRate/10;
				mbulletDistance = baseBulletDistance + property.AttackDistance;
				mbulletDamage = baseBulletDamage + property.Damage;

		}

		void checkForget(){
				if(baseBulletDamage==0){
						baseBulletDamage = 1;
				}
				if(baseBulletSpeed==0){
						baseBulletSpeed = 10;
				}
				if(baseBulletDistance==0){
						baseBulletDistance = 10;
				}
				if(baseBulletRate==0){
						baseBulletRate = 0.5f;
				}
				if(mdamageRate==0){
						baseBulletRate = 10f;
				}
		}
}
