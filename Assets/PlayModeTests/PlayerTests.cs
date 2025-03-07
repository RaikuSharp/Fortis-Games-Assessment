using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.Core;

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
    public IEnumerator PlayerJumpStateTest()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Initiation of PlayerJumpStateTest");
        var player = model.player;
        Assert.IsNotNull(player, "Could not find player.");
        
        Assert.AreEqual(PlayerController.JumpState.Grounded, player.jumpState);
        Debug.Log("Test finished");
        yield return null;
    }
}
