/// <summary>
/// By Deven Smith
/// 1/29/2014
/// ShadowScript2
/// made the shadows only move to positions the player has been 
/// instead of tyring to treat it like a player recieving delayed inputs
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum Facings {Crouch, Up, Right, Left, Idle};
public class ShadowScript2 : EntityScript 
{
	Animator aAnim;

	public Facings eFacing = Facings.Right;

	public bool bGrounded = false;

	public int iDelay = 60;

	public float fMoveSpeed = 5.0f;
	public float fGroundDistance = 0.2f;
	public float fKnockUpForce = 7500.0f;

	public Vector3 vDirection = Vector3.zero;

	int lmGroundLayer;

	public GameObject goSwapFX;
	public GameObject gPlayerAttackPrefab;
	public GameObject goCharacter;
	public GameObject[] goRightHandWeapons;
	public GameObject[] goLeftHandWeapons;
	public GameObject[] goCharacterModels; // katan / kama / kunai / naginata

	public string sGroundLayer;

	bool bSpirit = true;
	bool bSwordAttack = false;
	bool bRangedAttack = true;
	bool bRopeAttack = false;
	bool bNaginataAttack = false;
	bool bCrouch = false;
	bool bAttacking = false;
	bool bJumping = false;
	bool bMoved = false;
	bool bGoingLeft = true;
	bool bIdle = false;
	bool bLookUp = false;
	
	float fHeight = 0.0f;
	float fWidth = 0.0f;
	float fMaxAttackTime;
	float fCurAttackTime;
	float fMoveTime;

	List<Vector3> lvPositions = new List<Vector3>();


	// Use this for initialization
	void Start () 
	{
		lmGroundLayer = LayerMask.NameToLayer(sGroundLayer);
		aAnim = goCharacter.GetComponent<Animator>();
		for(int i = iDelay; i > 0; i--)
			lvPositions.Add(transform.position);

		CapsuleCollider myCollider = GetComponent<CapsuleCollider>();
		fHeight = myCollider.height;
		fWidth = myCollider.radius;

		Component[] components = goCharacter.GetComponentsInChildren(typeof(Rigidbody));
		foreach(Component c in components)
		{
			(c as Rigidbody).isKinematic = true;
		}

		foreach(GameObject go in goLeftHandWeapons)
		{
			go.SetActive(false);
		}
		foreach(GameObject go in goRightHandWeapons)
		{
			go.SetActive(false);
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		RaycastHit hit;
		if(Physics.Raycast(transform.position, -transform.up, out hit, fGroundDistance + fHeight/2, 1<<lmGroundLayer))
		{
			if(hit.collider.tag != "Ground")
			{
				bGrounded = false;
				bJumping = false;//true;
				bIdle = false;
			}
			else 
			{
				bGrounded = true;
				bJumping = false;
			}
		}
		else
		{
			if(!bJumping)
			{
				bIdle = false;
				bJumping = false;//true;
				ChangeFacing(4);
			}
			bGrounded = false;
		}

		if(eFacing == Facings.Right)
		{
			bGoingLeft = false;
			bIdle = false;
			bIdle = false;
			transform.eulerAngles = new Vector3(0, 180, 0);
		}
		if(eFacing == Facings.Left)
		{
			bGoingLeft = true;
			bIdle = false;
			bIdle = false;
			transform.eulerAngles = new Vector3(0, 0, 0);
		}
		if(eFacing == Facings.Crouch)
		{
			bCrouch = true;
			bIdle = false;
		}
		if(eFacing == Facings.Up)
		{
			bLookUp = true;
			bIdle = false;
		}
		if(eFacing == Facings.Idle)
		{
			if(bGrounded)
			{
				bIdle = true;
				bCrouch = false;
				bLookUp = false;
				bJumping = false;
			}
			else
			{
				bJumping = false;//true;
				if(bCrouch || bLookUp || bJumping)
				{
					bIdle = false;
				}
			}
		}

		if(bGoingLeft)
			transform.eulerAngles = new Vector3(0, 0, 0);
		else
			transform.eulerAngles = new Vector3(0, 180, 0);

		//if currently attacking resolve it
		if(fCurAttackTime > 0)
		{
			if(bRangedAttack)
			{
				fCurAttackTime -= Time.deltaTime;
			}
			else if (bSwordAttack)
			{
				fCurAttackTime -= Time.deltaTime;
			}
			else if(bRopeAttack)
			{
				fCurAttackTime -= Time.deltaTime;
			}
			else if (bNaginataAttack)
			{
				fCurAttackTime -= Time.deltaTime;
			}

			if(fCurAttackTime <= 0)
			{
				bAttacking = false;
			}
		}
		if(bMoved)
			bCrouch = false;

		SendAnimatorBools();
		if(fMoveTime != Time.time)
			bMoved = false;
	}

	public override void Move()
	{
		bMoved = true;
		bIdle = false;
		bLookUp = false;
		fMoveTime = Time.time;
		Vector3 vectorToPosition = lvPositions[0] - transform.position;
		transform.position = lvPositions[0];
		lvPositions.RemoveAt(0);
	}

	void AddPosition(Vector3 newPosition)
	{
		lvPositions.Add(newPosition);
	}

	void ChangeFacing(int newFacing)
	{

		switch(newFacing)
		{
		case 0:
			eFacing = Facings.Left;
			bGoingLeft = true;
			vDirection = new Vector3(1.0f, 0, 0);
			break;
		case 1:
			eFacing = Facings.Right;
			bGoingLeft = false;
			vDirection = new Vector3(-1.0f, 0, 0);
			break;
		case 2:
			eFacing = Facings.Up;
			bLookUp = true;
			bCrouch = false;
			vDirection = new Vector3(0, 1.0f, 0);
			break;
		case 3:
			eFacing = Facings.Crouch;
			bCrouch = true;
			bLookUp = false;
			vDirection = new Vector3(0, -1.0f, 0);
			break;
		case 4:
			eFacing = Facings.Idle;
			bIdle = true;
			bCrouch= false;
			bLookUp = false;

			if(transform.eulerAngles == new Vector3(0, 0, 0))
				vDirection = new Vector3(1.0f, 0, 0);
			else
				vDirection = new Vector3(-1.0f, 0, 0);
			break;
		default:
			eFacing = Facings.Right;
			break;
		}

		SetWeaponModels();

		foreach(GameObject go in goLeftHandWeapons)
		{
			go.collider.enabled = false;
		}
		foreach(GameObject go in goRightHandWeapons)
		{
			go.collider.enabled = false;
		}
	}

	void ChangeAttackTime(float a_fNewAttackTime)
	{
		fMaxAttackTime = a_fNewAttackTime;
	}

	void ChangeAttackMode(int a_iMode)
	{
		switch(a_iMode)
		{
		case 0:
			if(!bRangedAttack)
				Instantiate(goSwapFX, transform.position, goSwapFX.transform.rotation);

			foreach(GameObject go in goCharacterModels)
			{
				go.SetActive(false);
			}
			goCharacter = goCharacterModels[2];
			goCharacter.SetActive(true);
			aAnim = goCharacter.GetComponent<Animator>();

			bRangedAttack = true;
			bSwordAttack = false;
			bRopeAttack = false;
			bNaginataAttack = false;
			break;
		case 1:
			if(!bSwordAttack)
				Instantiate(goSwapFX, transform.position, goSwapFX.transform.rotation);
			foreach(GameObject go in goCharacterModels)
			{
				go.SetActive(false);
			}
			goCharacter = goCharacterModels[0];
			goCharacter.SetActive(true);
			aAnim = goCharacter.GetComponent<Animator>();

			bRangedAttack = false;
			bSwordAttack = true;
			bRopeAttack = false;
			bNaginataAttack = false;
			break;
		case 2:
			if(!bNaginataAttack)
				Instantiate(goSwapFX, transform.position, goSwapFX.transform.rotation);
			foreach(GameObject go in goCharacterModels)
			{
				go.SetActive(false);
			}
			goCharacter = goCharacterModels[3];
			goCharacter.SetActive(true);
			aAnim = goCharacter.GetComponent<Animator>();

			bRangedAttack = false;
			bSwordAttack = false;
			bRopeAttack = false;
			bNaginataAttack = true;
			break;
		case 3:
			if(!bNaginataAttack)
			Instantiate(goSwapFX, transform.position, goSwapFX.transform.rotation);
			foreach(GameObject go in goCharacterModels)
			{
				go.SetActive(false);
			}
			goCharacter = goCharacterModels[3];
			goCharacter.SetActive(true);
			aAnim = goCharacter.GetComponent<Animator>();

			bRangedAttack = false;
			bSwordAttack = false;
			bRopeAttack = false;
			bNaginataAttack = true;
			break;
		default:
			print("Excuse me but this is not a valid option");
			break;
		}
		//SetWeaponModels();
		foreach(GameObject go in goLeftHandWeapons)
		{
			go.collider.enabled = false;
		}
		foreach(GameObject go in goRightHandWeapons)
		{
			go.collider.enabled = false;
		}
	}


	public override void Attack()
	{
		bAttacking = true;
		if(bRangedAttack)
		{
			GameObject attack = (GameObject)Instantiate (gPlayerAttackPrefab, transform.position, gPlayerAttackPrefab.transform.rotation);
			attack.SendMessage ("SetDirection", vDirection, SendMessageOptions.DontRequireReceiver);
		}
		fCurAttackTime = fMaxAttackTime;
		SetWeaponModels();
	}


	public void RangedAttack(Vector3 a_vAttackDirection)
	{
		if(bRangedAttack)
		{
			bAttacking = true;
			GameObject attack = (GameObject)Instantiate (gPlayerAttackPrefab, transform.position, gPlayerAttackPrefab.transform.rotation);
			attack.SendMessage ("SetDirection", a_vAttackDirection, SendMessageOptions.DontRequireReceiver);
			fCurAttackTime = fMaxAttackTime;
		}
	}

	public override void Hurt(int aiDamage)
	{
		Instantiate(goSwapFX, transform.position, transform.rotation);
		gameObject.SendMessage("DeathSound", SendMessageOptions.DontRequireReceiver);
		gameObject.SetActive(false);
	}

	public void SetWeaponModels()
	{
		foreach(GameObject go in goLeftHandWeapons)
		{
			go.SetActive(false);
		}
		foreach(GameObject go in goRightHandWeapons)
		{
			go.SetActive(false);
		}
		
		if(bGoingLeft)
		{
			if(bSwordAttack)
				goLeftHandWeapons[0].SetActive(true);
			if(bNaginataAttack)
				goLeftHandWeapons[1].SetActive(true);
		}
		else
		{
			if(bSwordAttack)
				goRightHandWeapons[0].SetActive(true);
			if(bNaginataAttack)
				goRightHandWeapons[1].SetActive(true);
		}

		foreach(GameObject go in goLeftHandWeapons)
		{
			go.collider.enabled = true;
		}
		foreach(GameObject go in goRightHandWeapons)
		{
			go.collider.enabled = true;
		}

	}
	
	void SendAnimatorBools()
	{
		aAnim.SetBool("bSpirit", bSpirit);
		aAnim.SetBool("bLookUp", bLookUp);
		aAnim.SetBool("bCrouch", bCrouch);
		aAnim.SetBool("bAttacking", bAttacking);
		aAnim.SetBool("bJumping", bJumping);
		aAnim.SetBool("bMoved", bMoved);
		aAnim.SetBool("bGrounded", bGrounded);
		aAnim.SetBool("bGoingLeft", bGoingLeft);
		aAnim.SetBool("bNaginataAttack", bNaginataAttack);
		aAnim.SetBool("bRopeAttack", bRopeAttack);
		aAnim.SetBool("bSwordAttack", bSwordAttack);
		aAnim.SetBool("bRangedAttack", bRangedAttack);
	}
}
