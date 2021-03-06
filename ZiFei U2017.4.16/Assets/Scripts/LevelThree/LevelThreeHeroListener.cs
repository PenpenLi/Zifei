﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class LevelThreeHeroListener : MonoBehaviour 
{
	public static GameObject m_invisibleColliderObj = null;				//需要隐藏的碰撞盒
	public GameObject[] m_colliderObj;									//村子里的碰撞体
	public float m_heroWalkSpeed = 8f;									//主角移动速度
	public float m_jumpForceVertical = 800f;							//跳跃时垂直方向上向上的力
	public float m_jumpForceHorizontal = 300f;							//跳跃时水平方向上的力
	
	private Animator m_heroAnimator = null;								//主角上的animator组件
	private bool m_hideCollider = false;								//是否需要隐藏碰撞盒
	private bool m_collideLeftEdge = false;								//是否碰到左边界
	private bool m_collideRightEdge = false;							//是否碰到右边界

	private LevelThreeHeroController.heroHorizontalStates m_horizontalCurrentState = LevelThreeHeroController.heroHorizontalStates.idle;
	private LevelThreeHeroController.heroVerticalStates m_verticalCurrentState = LevelThreeHeroController.heroVerticalStates.idle;
	
	void OnEnable()														//对象可用时 加入到订阅者列表中
	{
		LevelThreeHeroController.onVerticalStateChange += OnVerticalStateChange;
		LevelThreeHeroController.onHorizontalStateChange += OnHorizontalStateChange;
		
	}
	void OnDisable()													//不可用时，从订阅者列表中退出
	{
		LevelThreeHeroController.onVerticalStateChange -= OnVerticalStateChange;
		LevelThreeHeroController.onHorizontalStateChange -= OnHorizontalStateChange;
		
	}
	
	void Start()
	{
		m_heroAnimator = this.GetComponent<Animator> ();				//获取主角的动画组件
	}
	
	void OnCollisionEnter2D(Collision2D coll) 							//检测主角边界碰撞	
	{
		if(coll.gameObject.tag=="EdgeLeft")								//碰到左边界
			m_collideLeftEdge = true;
		else if(coll.gameObject.tag=="EdgeRight")						//碰到右边界
			m_collideRightEdge = true;
	}

	void OnCollisionExit2D(Collision2D coll) 							//检测主角边界碰撞退出	
	{
		if(coll.gameObject.tag=="EdgeLeft")								//退出左边界碰撞
			m_collideLeftEdge = false;
		else if(coll.gameObject.tag=="EdgeRight")						//退出右边界碰撞
			m_collideRightEdge = false;
	}
	
	void LateUpdate()
    {
        OnHorizontalStateCycle();                                           //左右移动相关
        //ResetCollider();

        if (LevelThreeGameManager.Instance.GetHeroOnMovePanel(1))
        {
            if (m_verticalCurrentState == LevelThreeHeroController.heroVerticalStates.idle)
                this.transform.Translate(0f, LevelThreeGameManager.Instance.GetMovePanelSpeed(), 0f);
        }
        ChangeHeroSpeed();

    }

    private void ResetCollider()
    {
        for (int i = 0; i < m_colliderObj.Length; i++)                                          //遍历所有的地面(判断是否设置为碰撞体)
        {
            if (!m_hideCollider)                                            //如果当前不是下行状态
            {
                if (m_colliderObj[i].transform.position.y < this.transform.position.y)
                    m_colliderObj[i].SetActive(true);                   //显示所有低于主角的地面碰撞盒
                else
                    m_colliderObj[i].SetActive(false);                  //隐藏所有高于主角的地面碰撞盒
            }
            else                                        //如果当前为下行状态
            {
                if (m_invisibleColliderObj.name == m_colliderObj[i].name)   //只隐藏主角所站立的地面
                {
                    m_colliderObj[i].SetActive(false);
                }
            }
        }
    }

    private void ChangeHeroSpeed()
    {
        if (m_invisibleColliderObj != null && m_invisibleColliderObj.name == "groundCollider02")//如果主角站在岩浆上
        {
            LevelThreeGameManager.Instance.SetHeroBloodReduce(0.0025f); //血量值减少
            m_heroWalkSpeed = 4f;
        }
        else
            m_heroWalkSpeed = 8f;
    }

    void AnimationNumClose()											//关闭所有动画改变的变量
	{
		m_heroAnimator.SetInteger("idleRun", 2);
		m_heroAnimator.SetInteger("idleJump", 2);
		m_heroAnimator.SetInteger("idleFall", 2);
		m_heroAnimator.SetInteger("runFall", 2);
		m_heroAnimator.SetInteger("runJump", 2);
		m_heroAnimator.SetInteger("jumpFall", 2);
	}
	
	void OnHorizontalStateCycle()										//左右移动位移相关
	{
		if(LevelThreeGameManager.Instance.GetHeroOnMovePanel(0))
		{
			float _panelSpeed = LevelThreeGameManager.Instance.GetMovePanelSpeed();
			switch(m_horizontalCurrentState)
			{
			case LevelThreeHeroController.heroHorizontalStates.idle:
				this.transform.Translate(_panelSpeed, 0f, 0f);		
				break;
			case LevelThreeHeroController.heroHorizontalStates.left:
			case LevelThreeHeroController.heroHorizontalStates.right :
				m_heroWalkSpeed = 8f + _panelSpeed;
				break;
			}
		}
		Vector3 _localScale = this.transform.localScale;				//主角朝向
		switch(m_horizontalCurrentState)								//判断主角当前水平状态
		{
		case LevelThreeHeroController.heroHorizontalStates.left:				//当前为向左走状态
			if(!m_collideLeftEdge)										//当前未碰到左边界
			{
				
				this.transform.Translate(new Vector3((m_heroWalkSpeed*-1f)*0.02f, 0f, 0f));				//向左移动
				if(m_collideRightEdge)									//右边界变量归位
					m_collideRightEdge = false;
			}
			if(_localScale.x > 0f)										//主角朝向需更改
			{
				_localScale.x *= -1f;
				this.transform.localScale = _localScale;
			}
			break;
		case LevelThreeHeroController.heroHorizontalStates.right:			//当前为向右走状态
			if(!m_collideRightEdge)										//当前未碰到右边界
			{
				
				this.transform.Translate(new Vector3(m_heroWalkSpeed*0.02f, 0f, 0f));					//向右移动
				if(m_collideLeftEdge)									//左边界变量归位
					m_collideLeftEdge = false;
			}
			if(_localScale.x<0f)										//主角朝向需更改
			{
				_localScale.x *= -1f;
				this.transform.localScale = _localScale;
			}
			break;
		}
	}
	
	public void OnHorizontalStateChange(LevelThreeHeroController.heroHorizontalStates _horizontalNewState)	//主角状态改变时调用以改变动画
	{
		if(_horizontalNewState==m_horizontalCurrentState)				//当前水平状态未发生变化
			return;
		switch(_horizontalNewState)										//新水平状态类型
		{
		case LevelThreeHeroController.heroHorizontalStates.idle:				//需转为静止状态	
			if(m_verticalCurrentState!=LevelThreeHeroController.heroVerticalStates.falling&&					//当前状态不是跳跃或下落
			   m_verticalCurrentState!=LevelThreeHeroController.heroVerticalStates.jump)
				m_heroAnimator.SetInteger("idleRun", 0);
			break;
		case LevelThreeHeroController.heroHorizontalStates.left:				//需转为跑步状态
		case LevelThreeHeroController.heroHorizontalStates.right:	
			
			if(m_verticalCurrentState!=LevelThreeHeroController.heroVerticalStates.falling&&
			   m_verticalCurrentState!=LevelThreeHeroController.heroVerticalStates.jump)
				m_heroAnimator.SetInteger("idleRun", 1);
			break;
		} 
		m_horizontalCurrentState = _horizontalNewState;					//更新当前状态
        ResetCollider();
    }
	
	public void OnVerticalStateChange(LevelThreeHeroController.heroVerticalStates _verticalNewState)	//主角状态改变时调用以改变动画
	{
		if(_verticalNewState==m_verticalCurrentState)					//当前竖直状态未发生变化
			return;
		if(!CheckForValidVerticalState(_verticalNewState))				//当前树枝状态不能发生转变
			return;
		switch(_verticalNewState)										//新竖直状态类型
		{
		case LevelThreeHeroController.heroVerticalStates.idle:              //需转为静止状态
                if (m_verticalCurrentState == LevelThreeHeroController.heroVerticalStates.jump)
                {
                    AnimationNumClose();                                    //关闭所有变量
                    m_heroAnimator.SetInteger("idleJump", 0);               //idle <- jump

                }
                break;
		case LevelThreeHeroController.heroVerticalStates.jump:				//需转为跳跃状态
			if(m_horizontalCurrentState==LevelThreeHeroController.heroHorizontalStates.idle)			//如果当前水平未动
			{
				AnimationNumClose();									//关闭所有变量
				m_heroAnimator.SetInteger("idleJump", 1);				//idle→jump
			}
			else 														//如果当前为跑步状态
			{
				AnimationNumClose();									//关闭所有变量
				m_heroAnimator.SetInteger("runJump", 1);				//run→jump
				
			}
                if (GetComponent<Rigidbody2D>().velocity.y <= 0)
                    this.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, m_jumpForceVertical));						//给主角添加向上力
			break;
		case LevelThreeHeroController.heroVerticalStates.landing:			//需转为落地状态
			if(m_verticalCurrentState==LevelThreeHeroController.heroVerticalStates.falling)			//当前为下落状态
			{
				if(m_horizontalCurrentState==LevelThreeHeroController.heroHorizontalStates.idle)		//水平为空闲
				{
					if(m_heroAnimator.GetInteger("jumpFall")==1)
					{
						m_heroAnimator.SetInteger("jumpFall", 2);			//fall→idle
						m_heroAnimator.SetInteger("idleJump", 0);			//fall→idle
					}
					else
						AnimationNumClose();								//关闭所有变量
					m_heroAnimator.SetInteger("idleFall", 0);			//fall→idle
				}
				else 													//水平为跑步
				{
					if(m_heroAnimator.GetInteger("jumpFall")==1)
					{
						m_heroAnimator.SetInteger("jumpFall", 2);			//fall→idle
						m_heroAnimator.SetInteger("runJump", 0);			//fall→idle
					}
					else
						AnimationNumClose();								//关闭所有变量
					m_heroAnimator.SetInteger("runFall", 0);			//fall→run
				}
			}
			else if(m_verticalCurrentState==LevelThreeHeroController.heroVerticalStates.jump)		//当前为跳跃状态
			{
				if(m_horizontalCurrentState==LevelThreeHeroController.heroHorizontalStates.idle)		//水平为空闲
				{
					AnimationNumClose();								//关闭所有变量
					m_heroAnimator.SetInteger("idleJump", 0);			//jump→idle
				}
				else 													//水平为跑步
				{
					AnimationNumClose();								//关闭所有变量
					m_heroAnimator.SetInteger("runJump", 0);			//jump→run
				}
			}
			break;
		case LevelThreeHeroController.heroVerticalStates.down:				//需转为下行状态
			m_hideCollider = true;                                      //需要隐藏下方碰撞块
            ResetCollider();
            break;
		case LevelThreeHeroController.heroVerticalStates.falling:			//需转为下落状态	
			if(m_verticalCurrentState==LevelThreeHeroController.heroVerticalStates.jump)				//当前为跳跃状态
			{
				AnimationNumClose();									//关闭速游变量
				m_heroAnimator.SetInteger("jumpFall", 1);				//jump→fall
			}
			else 														//当前非跳跃状态
			{
				if(m_horizontalCurrentState==LevelThreeHeroController.heroHorizontalStates.idle)		//水平为空闲状态
				{
					AnimationNumClose();								//关闭所有变量
					m_heroAnimator.SetInteger("idleFall", 1);			//idle→fall
				}
				else 													//水平为跑步状态
				{
					AnimationNumClose();								//关闭所有变量
					m_heroAnimator.SetInteger("runFall", 1);			//run→fall
				}
			}
                if (m_horizontalCurrentState == LevelThreeHeroController.heroHorizontalStates.left ||      //防止跳跃后滑行
                    m_horizontalCurrentState == LevelThreeHeroController.heroHorizontalStates.right)
                {
                    m_heroAnimator.SetInteger("idleRun", 1);
                }
                m_hideCollider = false;                                     //关闭隐藏按钮
            ResetCollider();
            break;
		} 
		m_verticalCurrentState = _verticalNewState;						//更新竖直状态
	}
	
	bool CheckForValidVerticalState(LevelThreeHeroController.heroVerticalStates _newState)		//检测是否可以发生状态转变
	{
		bool _returnVal = false;											//默认不能
		switch(m_verticalCurrentState)										//判断当前竖直状态
		{
		case LevelThreeHeroController.heroVerticalStates.idle:					//当前为空闲或落地状态
			_returnVal = true;
			break;
		case LevelThreeHeroController.heroVerticalStates.landing:				
			if(_newState!=LevelThreeHeroController.heroVerticalStates.falling)	//不能转为下落状态
				_returnVal = true;
			else
				_returnVal = false;
			break;
		case LevelThreeHeroController.heroVerticalStates.jump:					//当前为跳跃状态
			if(_newState==LevelThreeHeroController.heroVerticalStates.falling||	//只能转为下落和落地状态
			   _newState==LevelThreeHeroController.heroVerticalStates.landing
               || _newState == LevelThreeHeroController.heroVerticalStates.idle)
				_returnVal = true;
			else
				_returnVal = false;
			break;
			
		case LevelThreeHeroController.heroVerticalStates.down:					//当前为下行状态
			if(_newState==LevelThreeHeroController.heroVerticalStates.falling)	//可转为下落状态
				_returnVal = true;
			else
				_returnVal = false;
			break;
		case LevelThreeHeroController.heroVerticalStates.falling:				//当前为下落状态
			if(_newState==LevelThreeHeroController.heroVerticalStates.landing)	//可转为落地状态
				_returnVal = true;
			else 
				_returnVal = false;
			break;
		}
		return _returnVal;													//返回是否可转化变量
	}
	
	
}

