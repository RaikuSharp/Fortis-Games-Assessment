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

public class PlayerTests : InputTestFixture
{
    Gamepad gamepad;
    PlatformerModel model = Simulation.GetModel<PlatformerModel>();

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        SceneManager.LoadScene("SampleScene");
        gamepad = InputSystem.AddDevice<Gamepad>();
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerJumpStateTest()
    {
        float timeout = 0f;
        Debug.Log("Initiation of PlayerJumpStateTest");

        yield return new WaitForSeconds(3);

        Assert.That(model.player.jumpState, Is.EqualTo(PlayerController.JumpState.Grounded));
        Press(gamepad.buttonSouth);
        yield return null;
        Debug.Log($"Jump state: {model.player.jumpState}");
        yield return new WaitForSeconds(0.5f);
        while (timeout < 3.0f && !model.player.IsGrounded)
        {
            Debug.Log($"Jump state: {model.player.jumpState}");
            timeout += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            Assert.That(model.player.jumpState, Is.Not.EqualTo(PlayerController.JumpState.Grounded));
        }
        Assert.That(model.player.jumpState, Is.EqualTo(PlayerController.JumpState.Landed));
        Debug.Log("Test finished");
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerEnemyCollisionTest()
    {
        Debug.Log("Initiating PlayerEnemyCollisionTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into enemy");
        while (model.player.health.IsAlive)
        {
            Move(gamepad.leftStick, new Vector2(1, 0));
            yield return null;
        }
        Assert.That(model.player.health.IsAlive, Is.False);
        Debug.Log("Player is dead");

        yield return new WaitForSeconds(3);
        Debug.Log("Test finished");
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerCollidesWithDeathBoxTest()
    {
        Debug.Log("Initiating PlayerCollidesWithDeathBoxTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into death zone");
        GameObject enemies = GameObject.Find("Enemies");
        enemies.SetActive(false);
        while (model.player.health.IsAlive)
        {
            Move(gamepad.leftStick, new Vector2(1, 0));
            yield return null;
        }
        Assert.That(model.player.health.IsAlive, Is.False);
        Debug.Log("Player is dead");
        yield return new WaitForSeconds(3);
        Debug.Log("Test finished");
    }

    [UnityTest]
    [Explicit, Category("PlayerController")]
    public IEnumerator PlayerCollectsTokenTest()
    {
        Debug.Log("Initiating PlayerCollectsTokenTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into token");
        var timeout = 4f;

        while (timeout > 0)
        {
            Move(gamepad.leftStick, new Vector2(1, 0));
            timeout -= Time.deltaTime;
            yield return null;
        }
        LogAssert.Expect(LogType.Log, "Token collected by player");
        Debug.Log("Test finished");
    }

    [UnityTest]
    [Explicit, Category("EnemyController")]
    public IEnumerator EnemyCollideWithTokenTest()
    {
        Debug.Log("Initiating EnemyCollideWithTokenTest");
        yield return new WaitForSeconds(5);
        Debug.Log("Walking into token");
        var timeout = 4f;
        var enemy = GameObject.Find("FirstEnemy").GetComponent<EnemyController>();
        var enemyAnimationController = enemy.GetComponent<AnimationController>();
        var tokenController = ScriptableObject.FindAnyObjectByType<TokenController>();

        Assert.That(tokenController, Is.Not.Null);

        GameObject player = GameObject.Find("Player");
        player.SetActive(false);

        var initialTokensCount = tokenController.tokens.Length;
        Debug.Log($"Initial tokens count: {initialTokensCount}");

        Debug.Log("Moving enemy to token");
        while (timeout > 0)
        {
            enemyAnimationController.move.x -= 0.05f;
            timeout -= Time.deltaTime;
            yield return null;
        }

        Assert.That(tokenController.tokens.Length, Is.EqualTo(initialTokensCount));
        Debug.Log("Test finished");
    }
}
