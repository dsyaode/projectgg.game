using UnityEngine;
using System.Collections;

public class Boss1Logic : enemylogic {


        private float mWalkTime = 2.0f;
        private float mInterWalkTime = 0.4f;
        private float mUseWalkTime = 0;

        private float mRestTime = 3;
        private float mUseTime = 0;


        private float mFollowTime = 3.0f;

        private int mStartFallStone = 0;
        private int mTotalFallStone = 5;

        private float mScaleMoveY = 2f;
        
        public enum Status { 
                Rest = 1,       //原地休息
                Walk = 2,       //小步走
                Sprint = 3,     //冲向主角
                Angry = 4,      //生气变大
                Follow = 5,     //跟随主角
                Jump = 6,       //跳跃

                StartJump = 7,  //开场跳出来
        }
        private Status mStatus;
        private float mAddX;
        private float mAddY;

        private float mWalkAcc;
        private float mWalkMaxSpeed;

        private float mSprintAcc;
        private float mSprintMaxSpeed;
        private float mAngrySprintMaxSpeed;

        private float mFollowAcc;
        private float mFollowMaxSpeed;

        private bool mHasAngry;
        private float mAngryPer;

        private bool mHasJump;
        private float mJumpTime;

        AudioSource mAudio;

        private float mShadowOffsetY;

        private float mColliderRadius;

        private float mJumpHeight;

        void Start() {
                mWalkAcc = 30;
                mWalkMaxSpeed = 2;

                mSprintAcc = 200;
                mSprintMaxSpeed = 10;
                mAngrySprintMaxSpeed = 40;

                mFollowAcc = 100;
                mFollowMaxSpeed = 6;

                mHasAngry = false;
                mAngryPer = 0.5f;

                mHasJump = false;
                mJumpTime = 0.4f;

                mShadowOffsetY = -1.7f;

                mJumpHeight = 20;

                SphereCollider collider = this.gameObject.GetComponent<SphereCollider>();
                mColliderRadius = collider.radius;

                //mStatus = Status.Walk;
                //setWalkAI();
                mStatus = Status.StartJump;
                setStartJumpAI();
        }

        public float getScale() {
                if (mHasAngry) {
                        return 3;
                }
                return 2;
        }

        private Status getNextStatus() { 
                switch(mStatus){
                        case Status.Rest:
                                if (mHasAngry) {
                                        return Status.Sprint;
                                } else {
                                        if (Random.Range(0, 1.0f) < 0.5f) {
                                                return Status.Walk;
                                        } else {
                                                return Status.Follow;
                                        }
                                        //return Status.Follow;
                                }
                                break;
                        case Status.Walk:
                                if (Random.Range(0, 1.0f) < 0.5f) {
                                        return Status.Sprint;
                                } else {
                                        return Status.Jump;
                                }
                                //return Status.Jump;
                                break;
                        case Status.Sprint:
                                return Status.Rest;
                                break;
                        case Status.Angry:
                                return Status.Sprint;
                        case Status.Follow:
                                return Status.Walk;
                                break;
                        case Status.Jump:
                                return Status.Rest;
                                break;
                        case Status.StartJump:
                                return Status.Walk;
                                break;
                }
                return Status.Walk;
        }
	
	// Update is called once per frame
	void Update () {
                switch (mStatus) {
                        case Status.Rest:
                                mUseTime = mUseTime - Time.deltaTime;
                                if (mUseTime <= 0) {
                                        changeNextStatus();
                                }
                                break;
                        case Status.Walk:
                                mUseTime = mUseTime - Time.deltaTime;
                                if (mUseTime <= 0) {
                                        changeNextStatus();
                                } else {
                                        mUseWalkTime = mUseWalkTime - Time.deltaTime;
                                        if (mUseWalkTime <= 0) {
                                                resetWalkDir();
                                        }
                                }

                                break;
                        case Status.Sprint:
                                break;
                        case Status.Angry:
                        case Status.Follow:
                                mUseTime = mUseTime - Time.deltaTime;
                                if (mUseTime <= 0) {
                                        changeNextStatus();
                                } else {
                                        updateFollowAcc();
                                }
                                break;
                }
	}

        public void changeNextStatus() {
                Status status = getNextStatus();
                changeToNextStatus(status);
        }

        public void setWalkAI() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();

                //spr.SetSprite("boss1_04");
                ani.Play("walk");
                mAudio = constant.getSoundLogic().playEffect("boss1_walk");
                resetWalkDir();

                mUseTime = mWalkTime;
        }

        public void resetWalkDir() {
                mUseWalkTime = mInterWalkTime;
                enemy_property pro = this.gameObject.GetComponent<enemy_property>();
                pro.BaseMoveSpeed = this.mWalkMaxSpeed;

                float d = Mathf.Deg2Rad * Random.Range(0, 359);

                mAddX = mWalkAcc * Mathf.Cos(d);
                mAddY = mWalkAcc * Mathf.Sin(d);
        }

        public void setRestAI() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();

                ani.Play("rest");

                mUseTime = mRestTime;

                enemy_property pro = this.gameObject.GetComponent<enemy_property>();
                pro.BaseMoveSpeed = 0;
        }

        public void setFollowAI() {
                GameObject player = constant.getPlayer();
                if (player == null) {
                        return;
                }

                enemy_property pro = this.gameObject.GetComponent<enemy_property>();
                pro.BaseMoveSpeed = this.mFollowMaxSpeed;

                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();

                //spr.SetSprite("boss1_01");
                ani.Play("follow");
                updateFollowAcc();


                mAudio = constant.getSoundLogic().playEffect("boss1_fly", false, 1);

                mUseTime = mFollowTime;
        }

        public void updateFollowAcc() {
                GameObject player = constant.getPlayer();
                float x = player.transform.position.x - this.transform.position.x;
                float y = player.transform.position.y - this.transform.position.y;

                SphereCollider collider = this.GetComponent<SphereCollider>();
                //Debug.Log("collider.bounds.size:" + collider.bounds.size.x + "," + collider.bounds.size.y);
                //Debug.Log("dis:" + x + "," + y);
                if (collider.bounds.size.x / 2 > Mathf.Abs(x)) {
                        x = 0;
                }
                if (collider.bounds.size.y / 2 > Mathf.Abs(y)) {
                        y = 0;
                }

                float d = Mathf.Atan2(y, x);
                //Debug.Log("X,Y,D:" + x + "," + y + "," + d);
                mAddX = mFollowAcc * Mathf.Cos(d);
                mAddY = mFollowAcc * Mathf.Sin(d);
        }

        public void stopAni() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();
                ani.Stop();
        }

        public void setJumpAI() {
                startJumpToSky();
        }

        private void squashStart1() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                float time = 0.1f;
                //播放弹性效果
                {
                        Hashtable args = new Hashtable();
                        args.Add("y", 0.5f);
                        args.Add("x", 1.25f);
                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeOutCirc);
                        args.Add("oncomplete", "squashEnd1");
                        args.Add("oncompletetarget", gameObject);

                        iTween.ScaleBy(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("y", -mScaleMoveY);

                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeOutCirc);

                        iTween.MoveBy(obj, args);
                }
        }

        private void squashEnd1() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                float time = 0.2f;
                float per = 1.5f;
                //播放弹性效果
                {
                        Hashtable args = new Hashtable();
                        args.Add("y", 2f * per);
                        args.Add("x", 0.8f / per);
                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeInCirc);
                        args.Add("oncomplete", "jumpToSky");
                        args.Add("oncompletetarget", gameObject);
                        iTween.ScaleBy(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("y", mScaleMoveY * per);

                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeInCirc);

                        iTween.MoveBy(obj, args);
                }
        }

        private void startJumpToSky() {
                squashStart1();
        }

        private void jumpToSky() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                //GameObject shadowObj = constant.getChildGameObject(this.gameObject, "Shadow");
                //tk2dSprite shadow = shadowObj.GetComponent<tk2dSprite>(); 
                enemyAniManager ani = this.GetComponent<enemyAniManager>();
                GameObject shadowObj = ani.getShadowObj();

                {
                        float y = obj.transform.position.y - obj.transform.localPosition.y;
                        Vector3 v = obj.transform.position;
                        v.z = -5;
                        obj.transform.position = v; //.z = 5;
                        Hashtable args = new Hashtable();
                        args.Add("y", y + mJumpHeight);

                        args.Add("time", mJumpTime);
                        //args.Add("easetype", iTween.EaseType.easeInCirc);
                        args.Add("oncomplete", "jumpMove");
                        args.Add("oncompletetarget", gameObject);
                        iTween.MoveTo(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("x", 0.2f);
                        args.Add("y", 0.2f);

                        args.Add("time", mJumpTime);
                        args.Add("easetype", iTween.EaseType.easeInCirc);

                        iTween.ScaleBy(shadowObj, args);
                }

                SphereCollider collider = this.gameObject.GetComponent<SphereCollider>();
                collider.radius = 0;

                setIsTrigger(true);
                constant.getSoundLogic().playEffect("boss1_jump");
        }

        private void jumpMove() {
                GameObject player = constant.getPlayer();
                this.gameObject.transform.position = player.transform.position;

                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");
                obj.transform.localScale = new Vector3(1, 1, 1);
                //obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y - 0.8f * (1.5f - 1), obj.transform.localPosition.z);
                jumpBackSky();
        }

        private void jumpBackSky() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");
                
                //GameObject shadowObj = constant.getChildGameObject(this.gameObject, "Shadow");
                enemyAniManager ani = this.GetComponent<enemyAniManager>();
                GameObject shadowObj = ani.getShadowObj();
                //tk2dSprite shadow = shadowObj.GetComponent<tk2dSprite>();

                {
                        float y = obj.transform.position.y;
                        Hashtable args = new Hashtable();
                        args.Add("y", y - mJumpHeight);

                        args.Add("time", mJumpTime);
                        args.Add("easetype", iTween.EaseType.easeInCubic);
                        args.Add("oncomplete", "finishJump");
                        args.Add("oncompletetarget", gameObject);
                        iTween.MoveTo(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("x", 5f);
                        args.Add("y", 5f);

                        args.Add("time", mJumpTime);
                        args.Add("easetype", iTween.EaseType.easeInCirc);

                        iTween.ScaleBy(shadowObj, args);
                }
        }

        private void squashStart() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");
                
                float time = 0.1f;
                //播放弹性效果
                {
                        Hashtable args = new Hashtable();
                        args.Add("y", 0.5f);
                        args.Add("x", 1.25f);
                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeOutCirc);
                        args.Add("oncomplete", "squashEnd");
                        args.Add("oncompletetarget", gameObject);

                        iTween.ScaleBy(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("y", -mScaleMoveY);

                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeOutCirc);

                        iTween.MoveBy(obj, args);
                }
        }

        private void squashEnd() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                float time = 0.2f;
                //播放弹性效果
                {
                        Hashtable args = new Hashtable();
                        args.Add("y", 2f);
                        args.Add("x", 0.8f);
                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeOutBack);
                        args.Add("oncomplete", "totalFinishJump");
                        args.Add("oncompletetarget", gameObject);
                        iTween.ScaleBy(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("y", mScaleMoveY);

                        args.Add("time", time);
                        args.Add("easetype", iTween.EaseType.easeOutBack);

                        iTween.MoveBy(obj, args);
                }
        }

        private void finishJump() {
                constant.getSoundLogic().playEffect("boss1_fall");
                setIsTrigger(false);
                scaleBackBoxCollider();
                constant.getMapLogic().normalShake();

                //createYellowWater();
                squashStart();
        }

        private void totalFinishJump() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                enemyAniManager ani = this.GetComponent<enemyAniManager>();
                GameObject shadowObj = ani.getShadowObj();

                Vector3 v = obj.transform.localPosition;
                v.z = 0;
                obj.transform.localPosition = v;

                createYellowWater();
                changeNextStatus();
        }

        public void setAngryAI() {
                iTween.Stop(this.gameObject);
                mHasAngry = true;

                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();

                ani.Play("angry");

                GameObject ui = constant.getChildGameObject(this.gameObject, "ui");
                //iTween.ScaleTo(ui, new Vector3(3, 3, 1), 0.5f);

                Hashtable args = new Hashtable();
                args.Add("x", 3);
                args.Add("y", 3);
                args.Add("z", 1);

                args.Add("time", 1.5f);
                args.Add("easetype", iTween.EaseType.easeInBounce);
                args.Add("oncomplete", "AnimationEnd");
                args.Add("oncompletetarget", gameObject);
                iTween.ScaleTo(ui, args);

                //播放生气音效
                soundLogic soundLogic = constant.getSoundLogic();
                soundLogic.playEffect("boss1_angry");

                //摄像机晃动
                maplogic logic = constant.getMapLogic();
                logic.shakeCamera();
        }

        public void AnimationEnd() {
                Debug.Log("AnimationEnd");
                //碰撞体变大
                SphereCollider collider = this.gameObject.GetComponent<SphereCollider>();
                //collider.size.Set(4,4,1);
                //collider.size = new Vector3(2.5,4,1);
                collider.radius = mColliderRadius * 1.5f;
                changeNextStatus();
        }

        public void setSprintAI() {
                GameObject player = constant.getPlayer();
                if (player == null) {
                        return;
                }

                enemy_property pro = this.gameObject.GetComponent<enemy_property>();
                pro.BaseMoveSpeed = this.mSprintMaxSpeed;
                if (this.mHasAngry) {
                        pro.BaseMoveSpeed = this.mAngrySprintMaxSpeed;
                }

                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                tk2dSpriteAnimator ani = obj.GetComponent<tk2dSpriteAnimator>();

                //spr.SetSprite("boss1_01");
                ani.Play("sprint");

                float x = player.transform.position.x - this.transform.position.x;
                float y = player.transform.position.y - this.transform.position.y;

                SphereCollider collider = this.GetComponent<SphereCollider>();
                Debug.Log("collider.bounds.size:" + collider.bounds.size.x + "," + collider.bounds.size.y);
                Debug.Log("dis:" + x + "," + y);
                if (collider.bounds.size.x / 2 > Mathf.Abs(x)) {
                        x = 0;
                }
                if (collider.bounds.size.y / 2 > Mathf.Abs(y)) {
                        y = 0;
                }

                float d = Mathf.Atan2(y, x);
                //Debug.Log("X,Y,D:" + x + "," + y + "," + d);
                mAddX = mSprintAcc * Mathf.Cos(d);
                mAddY = mSprintAcc * Mathf.Sin(d);

                mAudio = constant.getSoundLogic().playEffect("boss1_sprint");

                //Debug.Log("mAddX,mAddY:" + mAddX + "," + mAddY);
        }

        public void changeToNextStatus(Status status) {
                mStatus = status;
                //Debug.Log("changeToNextStatus：" + mStatus);
                stopAni();
                stopMove();
                stopFollowEffect();

                switch (mStatus) {
                        case Status.Rest:
                                setRestAI();
                                break;
                        case Status.Walk:
                                setWalkAI();
                                break;
                        case Status.Sprint:
                                setSprintAI();
                                break;
                        case Status.Angry:
                                setAngryAI();
                                break;
                        case Status.Follow:
                                setFollowAI();
                                break;
                        case Status.Jump:
                                setJumpAI();
                                break;
                        case Status.StartJump:
                                setStartJumpAI();
                                break;
                }
        }

        override public Vector3 getMoveAcc() {
                Vector3 v = new Vector3();
                v.x = 0;
                v.y = 0;
                v.z = 0;

                float mass = this.GetComponent<Rigidbody>().mass;

                switch (mStatus) {
                        case Status.Rest:
                                break;
                        case Status.Walk:
                                v.x = mAddX;
                                v.y = mAddY;
                                break;
                        case Status.Sprint:
                                v.x = mAddX;
                                v.y = mAddY;
                                break;
                        case Status.Angry:
                        case Status.Follow:
                                v.x = mAddX;
                                v.y = mAddY;
                                break;
                }
                v.x = v.x * mass;
                v.y = v.y * mass;
                return v;
        }

        override public void beAttack(GameObject obj) {
                if (mStatus == Status.Angry) { 
                        return;
                }
 
                base.beAttack(obj);
                if (!mHasAngry) {
                        checkAngry();
                }
        }

        public void checkAngry() {
                enemy_property pro = this.gameObject.GetComponent<enemy_property>();
                float curPer = ((float)pro.Hp)/pro.MaxHp;
                if (curPer <= mAngryPer) {
                        changeToNextStatus(Status.Angry);
                }
        }

        private void OnCollisionEnter(Collision collision) {
                //Debug.Log("OnCollisionEnter:" + collision.gameObject.name);
                onCollision(collision);
        }

        private void OnCollisionExit(Collision collision) {
                //Debug.Log("OnCollisionEnter:" + collision.gameObject.name);
                //onCollision(collision);
                stopMove();

        }

        //void OnCollisionStay(Collision collision) {
        //        onCollision(collision);
        //}

        private void onCollision(Collision collision) {
                if (collision.gameObject.tag == constant.TAG_PLAYER) {
                        pushPlayer();
                }
                if (mStatus == Status.Sprint) {
                        constant.getSoundLogic().playEffect("boss1_attack");
                        startFall();
                        changeNextStatus();
                } else if (mStatus == Status.Follow) {
                        constant.getSoundLogic().playEffect("boss1_attack");
                        changeNextStatus();
                }

        }

        private void pushPlayer() {
                maplogic logic = constant.getMapLogic();
                logic.pushChar(this.gameObject);
        }

        private void startFall() {
                int num = 5;
                for (int i = 0; i < 5; ++i) {
                        float deltaTime = Random.Range(0,1.0f);
                        InvokeRepeating("fallStone", deltaTime, 0);  
                }
        }

        private void setIsTrigger(bool ret) {
                SphereCollider collider = this.gameObject.GetComponent<SphereCollider>();
                collider.isTrigger = ret;
                //collider.active = !ret;
        }

        private void scaleBackBoxCollider() {
                SphereCollider collider = this.gameObject.GetComponent<SphereCollider>();

                //collider.size = new Vector3(0, 0, 1);
                collider.radius = 0;

                InvokeRepeating("finshScaleBackBoxCollider", 0f, 0.01f);
        }

        private void finshScaleBackBoxCollider() {
                SphereCollider collider = this.gameObject.GetComponent<SphereCollider>();
                float x = collider.radius;
                float maxX = mColliderRadius;
                if (mHasAngry) {
                        maxX = mColliderRadius * 1.5f;
                }
                x = x + 0.1f;
                if (x > maxX) {
                        x = maxX;
                        CancelInvoke("finshScaleBackBoxCollider");
                }
                collider.radius = x;
        }

        private void fallStone() {
                Rect rect = constant.getFloorRect();
                //Debug.Log("fallStone:" + rect.xMin + "," + rect.xMax + "," + rect.yMin + "," + rect.yMax);
                Vector2 v2 = constant.getRandomPosInFloor();
                Vector3 v = new Vector3(v2.x, v2.y, -1);
                string fallStonePrefabStr = "Prefabs/scene/fallstone";
                GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load(fallStonePrefabStr), v, Quaternion.identity);

                BoxCollider boxCollider = clone.GetComponent<BoxCollider>();
                float w = boxCollider.bounds.size.x;
                float h = boxCollider.bounds.size.y;
                if (v.x - w / 2 < rect.xMin) {
                        v.x = rect.xMin + w / 2;
                } else if (v.x + w / 2 > rect.xMax) {
                        v.x = rect.xMax - w / 2;
                }

                if (v.y - h / 2 < rect.yMin) {
                        v.y = rect.yMin + h / 2;
                } else if (v.y + h / 2 > rect.yMax) {
                        v.y = rect.yMax - h / 2;
                }
                //clone.transform.position = v;
        }

        private void stopFollowEffect() {
                if (mAudio != null) {
                        constant.getSoundLogic().stopEffect(mAudio);
                        mAudio = null;
                }
        }

        private void createYellowWater() {
                string waterPrefabStr = "Prefabs/scene/yellowwater";
                Vector3 v = this.gameObject.transform.position;
                v.z = 0.1f;
                v.y = v.y + mShadowOffsetY;
                GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load(waterPrefabStr), v, Quaternion.identity);
        }

        private void endStartJump() {
                maplogic logic = constant.getMapLogic();
                logic.setPlayerCanCotroll(true);

                finishJump();
        }

        private void startStartJump() {
                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                enemyAniManager ani = this.GetComponent<enemyAniManager>();
                GameObject shadowObj = ani.getShadowObj();

                {
                        float y = obj.transform.position.y;
                        Hashtable args = new Hashtable();
                        args.Add("y", y - mJumpHeight);

                        args.Add("time", mJumpTime);
                        args.Add("easetype", iTween.EaseType.easeInCubic);
                        args.Add("oncomplete", "endStartJump");
                        args.Add("oncompletetarget", gameObject);
                        iTween.MoveTo(obj, args);
                }

                {
                        Hashtable args = new Hashtable();
                        args.Add("x", 5f);
                        args.Add("y", 5f);

                        args.Add("time", mJumpTime);
                        args.Add("easetype", iTween.EaseType.easeInCirc);

                        iTween.ScaleBy(shadowObj, args);
                }
        }
        private void setStartJumpAI() {
                maplogic logic = constant.getMapLogic();
                logic.setPlayerCanCotroll(false);

                GameObject obj = constant.getChildGameObject(this.gameObject, "AnimatedSprite");

                enemyAniManager ani = this.GetComponent<enemyAniManager>();
                GameObject shadowObj = ani.getShadowObj();

                Vector2 v = constant.getMiddlePos();
                this.transform.position = new Vector3(v.x, v.y, obj.transform.position.z);


                obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y + mJumpHeight, obj.transform.position.z);
                shadowObj.transform.localScale = new Vector3(obj.transform.localScale.x * 0.2f, obj.transform.localScale.y * 0.2f, obj.transform.localScale.z);

                startJumpFallStone();
                //startStartJump();
        }

        private void jumpFllStoneFloor() {

                //mStartFallStone = mStartFallStone - 1;
                //if (mStartFallStone < 1) {
                //        CancelInvoke("jumpFllStoneFloor");
                //        return;
                //}
                Vector2 center = constant.getMiddlePos();
                int t = 8;
                float startR = 1;
                float w_interval = 1;

                float r = startR + w_interval * mStartFallStone;
                //for (int j = 0; j < t; ++j) {
                //        //float d = Mathf.Deg2Rad * Random.Range(0, 359);
                //        float k = mStartFallStone % 2 == 1 ? 0.5f : 0;
                //        float y = Mathf.Sin(Mathf.Deg2Rad * (360.0f / t * (j+k))) * r;
                //        float x = Mathf.Cos(Mathf.Deg2Rad * (360.0f / t * (j+k))) * r;

                //        Vector3 v = new Vector3(center.x + x, center.y+y, -1);
                //        string fallStonePrefabStr = "Prefabs/scene/fallstone";
                //        GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load(fallStonePrefabStr), v, Quaternion.identity);

                //}

                //float y = Mathf.Sin(Mathf.Deg2Rad * (360.0f / t * (j + k))) * r;
                //float x = Mathf.Cos(Mathf.Deg2Rad * (360.0f / t * (j + k))) * r;
                float r1 = Random.Range(1.0f, 6.0f) ;
                float p = Random.Range(0, 2*Mathf.PI);
                float x = r1 * Mathf.Cos(p);
                float y = r1 * Mathf.Sin(p);
                Vector3 v = new Vector3(center.x + x, center.y + y, -1);
                string fallStonePrefabStr = "Prefabs/scene/fallstone";
                GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load(fallStonePrefabStr), v, Quaternion.identity);
        }

        private void startJumpFallStone() {
                Rect rect = constant.getFloorRect();

                Vector2 v2 = constant.getMiddlePos();
                int f = 5;
                float intervalTime = 0.5f;

                mStartFallStone = mTotalFallStone;
                //for (int i = f; i > 0; --i) {
                //        {
                //                mStartFallStone = i;
                //                Invoke("jumpFllStoneFloor", intervalTime*i);
                //        }
                //}
                //InvokeRepeating("jumpFllStoneFloor", intervalTime, intervalTime);
                for (int i = 0; i < 30; ++i) {
                        float time = Random.Range(0, intervalTime * f);
                        Invoke("jumpFllStoneFloor", time);
                }
                Invoke("startStartJump", intervalTime * f+1);
                
        }
}
