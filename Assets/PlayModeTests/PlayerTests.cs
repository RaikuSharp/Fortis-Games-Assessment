using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.Core;
using Platformer.Gameplay;
using System;
using NUnit.Framework.Internal;

/// <summary>
/// Test class for the PlayerController class.
/// </summary>
public class PlayerTests : InputTestFixture
{
    Gamepad gamepad;
    PlatformerModel model = Simulation.GetModel<PlatformerModel>();

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        // Loading SampleScene provided by the project
        SceneManager.LoadScene("SampleScene");
        gamepad = InputSystem.AddDevice<Gamepad>();
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerJumpStateTest()
    {
        float timeout = 0f;
        Debug.Log("Initiation of PlayerJumpStateTest");
        // Wait for the scene to load
        yield return new WaitForSeconds(3);
        // initial jump state should be grounded
        Assert.That(model.player.jumpState, Is.EqualTo(PlayerController.JumpState.Grounded));

        // Press the jump button
        Press(gamepad.buttonSouth);
        yield return null;
        // Logging the current jump state
        Debug.Log($"Jump state: {model.player.jumpState}");
        // give the player some time to jump, otherwise might fail
        yield return new WaitForSeconds(0.5f);

        // monitor the jump state for 3 seconds and while the player is airborne
        while (timeout < 3.0f && !model.player.IsGrounded)
        {
            Debug.Log($"Jump state: {model.player.jumpState}");
            timeout += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            Assert.That(model.player.jumpState, Is.Not.EqualTo(PlayerController.JumpState.Grounded));
        }
        //The last assertion should be the last state of the player, which should be landed
        Assert.That(model.player.jumpState, Is.EqualTo(PlayerController.JumpState.Landed));
        Assert.Pass("PlayerJumpStateTest passed");
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerEnemyCollisionTest()
    {
        Debug.Log("Initiating PlayerEnemyCollisionTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into enemy");

        // Move the player while they're alive
        while (model.player.health.IsAlive)
        {
            Move(gamepad.leftStick, new Vector2(1, 0));
            yield return null;
        }

        // the test should only get to this point if the player collided, and was killed, with the enemy
        Assert.That(model.player.health.IsAlive, Is.False);
        Assert.Pass("PlayerEnemyCollisionTest passed");
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerCollidesWithDeathBoxTest()
    {
        Debug.Log("Initiating PlayerCollidesWithDeathBoxTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into death zone");

        // Since the SampleScene is populated with enemies, we need to disable them to let the player run into the death zone
        GameObject enemies = GameObject.Find("Enemies");
        enemies.SetActive(false);

        // the remaining code is similar to the PlayerEnemyCollisionTest
        while (model.player.health.IsAlive)
        {
            Move(gamepad.leftStick, new Vector2(1, 0));
            yield return null;
        }
        Assert.That(model.player.health.IsAlive, Is.False);
        Assert.Pass("PlayerCollidesWithDeathBoxTest passed");
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerCollectsTokenTest()
    {
        Debug.Log("Initiating PlayerCollectsTokenTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into token");
        var timeout = 4f;

        // Similarly to the previous tests, we're making the player move a certain duration
        while (timeout > 0)
        {
            Move(gamepad.leftStick, new Vector2(1, 0));
            timeout -= Time.deltaTime;
            yield return null;
        }

        // This test uses the LogAssert.Expect method as means to quickly verify that the token was collected.
        LogAssert.Expect(LogType.Log, "Token collected by player");
        Assert.Pass("PlayerCollectsTokenTest passed");
    }

    [UnityTest]
    [Explicit, Category("EnemyController")]
    public IEnumerator EnemyCollideWithTokenTest()
    {
        // This test is within this collection because it requires the SampleScene to be loaded, however can be excluded if it's Category isn't included in the test run
        Debug.Log("Initiating EnemyCollideWithTokenTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into token");
        var timeout = 4f;

        // modified one of the enemies in the scene to be FirstEnemy
        var enemy = GameObject.Find("FirstEnemy").GetComponent<EnemyController>();
        var enemyAnimationController = enemy.GetComponent<AnimationController>();

        // the token controller is what stores the tokens in the scene
        var tokenController = GameObject.Find("Level").GetComponent<TokenController>();

        Assert.That(tokenController, Is.Not.Null);

        // disabling the player to let the enemy move to the token
        GameObject player = GameObject.Find("Player");
        player.SetActive(false);

        // store the initial token count to compare later
        var initialTokensCount = tokenController.tokens.Length;
        Debug.Log($"Initial tokens count: {initialTokensCount}");

        Debug.Log("Moving enemy to token");
        while (timeout > 0)
        {
            enemyAnimationController.move.x -= 0.05f / Time.deltaTime;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // the test should only get to this point if the enemy wasn't able to collect the token
        Assert.That(tokenController.tokens.Length, Is.EqualTo(initialTokensCount));
        Assert.Pass("EnemyCollideWithTokenTest passed");
    }
}
