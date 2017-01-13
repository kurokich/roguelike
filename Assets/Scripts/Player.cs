using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MovingObject {

    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    private Animator animator;
    private int food;
    private Vector2 touchOrigin = -Vector2.one;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    // Use this for initialization
    protected override void Start () {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        
        foodText.text = "Food: " + food;
        
        base.Start();
	}

    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;
        //キーボードでの操作
        #if UNITY_STANDALONE || UNITY_WEBPLAYER
        horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        vertical = (int)(Input.GetAxisRaw("Vertical"));
        //タッチ操作        
#else        
			//タッチ数が1以上、つまり画面がタッチされたら
		if (Input.touchCount > 0) {
            
			Touch myTouch = Input.touches[0];
            Debug.Log("aaa");    
			//タッチ開始時なら
			if (myTouch.phase == TouchPhase.Began) {
				//touchOriginにタッチ位置を取得
				touchOrigin = myTouch.position;
			}
			//画面から指を離した＆Xの位置(横向き)が0以上なら
			else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				//指を離した位置を取得
				Vector2 touchEnd = myTouch.position;
				//touchEnd.xからtouchOrigin.xを引き、横の移動幅を出す
				float x = touchEnd.x - touchOrigin.x;
				//touchEnd.yからtouchOrigin.yを引き、縦の移動幅を出す
				float y = touchEnd.y - touchOrigin.y;
				//このif文に再度入らないようにtouchOrigin.xを変更
				touchOrigin.x = -1;
				//移動幅xの絶対値のほうが長い時はhorizontal(横)を設定
				if (Mathf.Abs(x) > Mathf.Abs(y))
					//xが0より大きければ1(右移動)、小さければ-1(左移動)
					horizontal = x > 0 ? 1 : -1;
				//移動幅yの絶対値のほうが長い時はvertical(縦)を設定
				else
					//yが0より大きければ1(上移動)、小さければ-1(下移動)
					vertical = y > 0? 1 : -1;
			}
		}
		//プラットフォーム判定の終了
#endif
        if (horizontal != 0)
        {
            vertical = 0;
        }

        if (horizontal != 0 || vertical != 0)
        {
            AttemptMove<Wall>(horizontal, vertical);
        }
    }
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //Every time player moves, subtract from food points total.
        food--;
        foodText.text = " Food: " + food;
        //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
        base.AttemptMove<T>(xDir, yDir);

        //Hit allows us to reference the result of the Linecast done in Move.
        RaycastHit2D hit;

        if(Move(xDir,yDir,out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckIfGameOver();

        //Set the playersTurn boolean of GameManager to false now that players turn is over.
        GameManager.instance.playersTurn = false;
    }


    //OnCantMove overrides the abstract function OnCantMove in MovingObject.
    //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
    protected override void OnCantMove<T>(T component)
    {
        //Set hitWall to equal the component passed in as a parameter.
        Wall hitWall = component as Wall;

        //Call the DamageWall function of the Wall we are hitting.
        hitWall.DamageWall(wallDamage);

        //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
        animator.SetTrigger("pAttack");
    }


    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check if the tag of the trigger collided with is Exit.
        if (other.tag == "Exit")
        {
            Debug.Log("exit");
            //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
            Invoke("Restart", restartLevelDelay);

            //Disable the player object since level is over.
            enabled = false;
        }

        //Check if the tag of the trigger collided with is Food.
        else if (other.tag == "Food")
        {
            //Add pointsPerFood to the players current food total.
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
            //Disable the food object the player collided with.
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }

        //Check if the tag of the trigger collided with is Soda.
        else if (other.tag == "Soda")
        {
            //Add pointsPerSoda to players food points total
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }


    //Restart reloads the scene when called.
    private void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }


    public void LoseFood(int loss)
    {
        animator.SetTrigger("pDamage");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;
        CheckIfGameOver();
    }


    //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
    private void CheckIfGameOver()
    {
        //Check if food point total is less than or equal to zero.
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }
}
