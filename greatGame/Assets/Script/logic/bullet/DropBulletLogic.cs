using UnityEngine;
using System.Collections;

public class DropBulletLogic : MonoBehaviour {

        private Vector3 mDesPos;
        private Vector3 mSrcPos;
        private float mTime;
        private float mHeight;

        private float mFlyTime;
        private float mAcc;

        public void initDropBullet(Vector3 srcPos, Vector3 desPos, float time, float Height) {
                mSrcPos = srcPos;
                mDesPos = desPos;
                mTime = time;
                mHeight = Height;
        }
	// Use this for initialization
	void Start () {
                mFlyTime = 0;
                mAcc = mHeight * 2 / ((mTime / 2) * (mTime / 2));

                startMove();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

        private void startMove() {
                {
                        Hashtable ht = new Hashtable();

                        this.gameObject.transform.position = mSrcPos;

                        ht.Add("position", mDesPos);
                        ht.Add("time", mTime);
                        ht.Add("easetype", iTween.EaseType.linear);
                        ht.Add("onupdate", "bulletFly");
                        ht.Add("oncomplete", "finishFly");
                        iTween.MoveTo(this.gameObject, ht);
                }

                
        }

        private void bulletFly() {
                mFlyTime = mFlyTime + Time.deltaTime;

                float t = mFlyTime;
                float v = mAcc * mTime/2;

                if (mFlyTime < mTime / 2) {
                        t = mFlyTime;
                } else {
                        t = mTime - mFlyTime;
                }
                float h = v*t - mAcc* t*t/2;

                //GameObject obj = constant.getChildGameObject(this.gameObject, "ui");
                //obj.transform.localPosition = new Vector3(0, h, obj.transform.localPosition.z);

                GameObject bulletPic = constant.getChildGameObject(this.gameObject, "bulletPic");
                bulletPic.transform.localPosition = new Vector3(0, h, bulletPic.transform.localPosition.z);
        }

        private void finishFly() {
                GameObject.Destroy(this.gameObject);
        }
}
