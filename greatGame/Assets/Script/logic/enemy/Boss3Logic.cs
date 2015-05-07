using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss3Logic : enemylogic {

	// Use this for initialization
        public class MusicInfo {
                string prefab;
                float interval;
                float delta;
                float length;
                AudioSource mAudio;

                public string Prefab { get { return prefab; } set { prefab = value; } }
                public AudioSource Audio { get { return mAudio;} set { mAudio = value;} }
                public float Interval { get { return interval; } set { interval = value; } }
                public float Delta { get { return delta; } set { delta = value; } }
                public float Length { get { return length; } set { length = value; } }

                public MusicInfo(string prefab, float interval, float delta, float length) {
                        this.prefab = prefab;
                        this.interval = interval;
                        this.delta = delta;
                        this.length = length;
                }

        }

        public enum Status {
                Attack = 1,    //
                Run = 2,       //
                Start = 3,
        }

        //public List<MusicInfo> mMusicInfos;
        private Status mStatus;

        private float mRunAcc;
        private float mRunMaxSpeed;

        private float mX;
        private float mY;
        private float mAddX;
        private float mAddY;

        private MusicInfo mCurMusicInfo;

	void Start () {
                mCurMusicInfo = new MusicInfo("boss3/music3", 60 * 4 / 150, 0, 59);

                //{
                //        MusicInfo info = new MusicInfo("boss3/music2", 60 * 4 / 150, 0);
                //        mMusicInfos.Add(info);
                //}

                mRunAcc = 100;
                mRunMaxSpeed = 6;
                enemy_property pro = this.gameObject.GetComponent<enemy_property>();
                pro.BaseMoveSpeed = this.mRunMaxSpeed;

                changeToStatus(Status.Attack);

                //停止播放背景音乐
                soundLogic.getInstance().stopBackGround();
                //soundLogic.getInstance().BkVal = 0.02f;

                enemyShotBullet shooter = gameObject.GetComponent<enemyShotBullet>();
                shooter.upgradeProperties(pro);

                //soundLogic.getInstance().playBackGround("boss3/music3");
	}
	
	// Update is called once per frame
	void Update () {
                switch (mStatus) {
                        case Status.Run:
                                checkFinishRun();
                                break;
                        case Status.Attack:
                                checkFinishAttack();
                                break;
                }
	}

        override public Vector3 getMoveAcc() {
                Vector3 v = new Vector3(); 
                v.x = 0;
                v.y = 0;
                v.z = 0;

                if (mStatus == Status.Run) {
                        v.x = mAddX;
                        v.y = mAddY;
                }
                return v;
        }

        private Status getNextStatus() {
                switch (mStatus) {
                        case Status.Attack:
                                return Status.Run;
                                break;
                        case Status.Run:
                                return Status.Attack;
                        case Status.Start:
                                return Status.Attack;
                                break;
                }
                return Status.Run;
        }

        private void changeNextStatus() {
                Status status = getNextStatus();
                changeToStatus(status);
        }

        private void changeToStatus(Status status) {
                stopCurMusic();
                stopShotBullets();

                mStatus = status;
                switch (mStatus) {
                        case Status.Run:
                                setRunAI();
                                break;
                        case Status.Attack:
                                setAttackAI();
                                break;
                        case Status.Start:
                                setStartAI();
                                break;
                }
        }

        private void setRunAI() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");
                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();
                ani.Play("run");

                Rect rect = constant.getFloorRect();
                BoxCollider box = this.gameObject.GetComponent<BoxCollider>();

                mX = Random.Range(rect.xMin + box.size.x / 2, rect.xMax - box.size.x / 2);
                mY = Random.Range(rect.yMin + box.size.y / 2, rect.yMax - box.size.y / 2);

                //if (mX - box.size.x / 2 < rect.xMin) {
                //        mX = rect.xMin + box.size.x / 2;
                //}
                //if (mX + box.size.x / 2 > rect.xMax) {
                //        mX = rect.xMax + box.size.x / 2;
                //}
                //if (mY - box.size.y / 2 < rect.yMin) {
                //        mY = rect.yMin + box.size.y / 2;
                //}
                //if (mY + box.size.y / 2 > rect.yMax) {
                //        mY = rect.yMax - box.size.y / 2;
                //}

                float x = this.transform.position.x;
                float y = this.transform.position.y;
                float d = Mathf.Atan2(mY-y, mX-x);

                mAddX = mRunAcc * Mathf.Cos(d);
                mAddY = mRunAcc * Mathf.Sin(d);

                Debug.Log("setRunAi:" + mX + "," + mY + "; rect:" + rect.xMin + "," + rect.xMax + ","
                        + "," + rect.yMin + "," + rect.yMax);
        }

        private void stopCurMusic() {
                if (mCurMusicInfo != null && mCurMusicInfo.Audio != null) {
                        constant.getSoundLogic().stopEffect(mCurMusicInfo.Audio);
                }
        }

        private void setAttackAI() {
                stopCurMusic();

                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");
                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();
                ani.Play("attack");

                //int index = Random.Range(0, mMusicInfos.Count);
                //mCurMusicInfo = mMusicInfos[index];

                AudioSource src = constant.getSoundLogic().playEffect(mCurMusicInfo.Prefab);
                mCurMusicInfo.Audio = src;
                src.Play();
                InvokeRepeating("attackBullets", mCurMusicInfo.Delta, mCurMusicInfo.Interval);
        }

        private void stopShotBullets() {
                CancelInvoke("attackBullets");
        }

        private void attackBullets() {
                //for (int i = 0; i < 4; ++i) {
                //        enemyShotBullet bullet = this.gameObject.GetComponent<enemyShotBullet>();
                //        float dir = Mathf.PI / 2 * i;
                //        bullet.shootBullet(dir);
                //}
                dropBullets();
        }

        private void checkFinishRun() {
                float x = this.transform.position.x;
                float y = this.transform.position.y;

                //Debug.Log("X:" + x + "," + mX + "," + mAddX);
                if (mAddX != 0 && (x - mX)*mAddX >= 0 ) {
                        mAddX = 0;
                }

                if (mAddY != 0 && (y - mY) * mAddY >= 0) {
                        mAddY = 0;
                }

                if (mAddX == 0 && mAddY == 0) {
                        changeNextStatus();
                }
        }

        private void checkFinishAttack() {
                if (mCurMusicInfo == null) {
                        return;
                }
                if (!mCurMusicInfo.Audio.isPlaying) {
                        changeNextStatus();
                }
        }

        void onDestroy() {
                stopCurMusic();
        }

        private void setStartAI() {
                maplogic mapLogic = constant.getMapLogic();
                mapLogic.setPlayerCanCotroll(false);

                Vector2 v = constant.getMiddlePos();
                this.transform.position = new Vector3(v.x, v.y, this.transform.position.z);

                Light light = constant.getLight();
                light.type = LightType.Spot;
                light.range = 30f;
                light.spotAngle = 0;
                light.intensity = 0.1f;

                Hashtable ht = new Hashtable();
                ht.Add("from", 0);
                ht.Add("to", 70f);
                ht.Add("time", 3f);
                ht.Add("onupdate", "updateLight");
                ht.Add("oncomplete", "finishLight");
                iTween.ValueTo(this.gameObject, ht);
        }

        public void updateLight(float value) {
                //Debug.Log("updateLight:" + value);
                Light light = constant.getLight();
                //light.type = LightType.Spot;
                //light.range = value;
                light.spotAngle = value;
        }

        private void finishLight() {
                maplogic mapLogic = constant.getMapLogic();
                mapLogic.setPlayerCanCotroll(true);

                mapLogic.setNormalLight();
                changeNextStatus();
        }

        void OnCollisionEnter(Collision collision) {
                //if(collision.gameObject.layer == )
                if (collision.gameObject.tag == constant.TAG_PLAYER) {
                        pushPlayer();
                }
        }

        private void pushPlayer() {
                maplogic logic = constant.getMapLogic();
                logic.pushChar(this.gameObject);
        }

        private void dropBullets() {
                int num  = 5;
                for (int i = 0; i < num; ++i) {
                        string prefab = "Prefabs/bullets/dropBullet";
                        Vector3 v = this.gameObject.transform.position;
                        GameObject dropBullet = (GameObject)GameObject.Instantiate(Resources.Load(prefab), v, Quaternion.identity);

                        DropBulletLogic logic = dropBullet.GetComponent<DropBulletLogic>();

                        Vector3 srcPos = v;
                        Vector2 desPosV2 = constant.getRandomPosInFloor();
                        Vector3 desPos = new Vector3(desPosV2.x, desPosV2.y, srcPos.z);
                        float time = mCurMusicInfo.Interval;
                        float height = 3;
                        logic.initDropBullet(srcPos, desPos, time, height);
                }
        }
}
