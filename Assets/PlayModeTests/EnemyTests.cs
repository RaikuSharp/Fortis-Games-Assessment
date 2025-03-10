using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.Tilemaps;

public class EnemyControllerTests
{
    [SetUp]
    public void Setup()
    {
        // This test suite uses the sandbox scene.
        SceneManager.LoadScene("SandBox");
    }

    /// <summary>
    /// Wasn't part of the original test suite, but was added to understand the enemy pathing feature and creating a sandbox scene for it.
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator PathingScenrioTest()
    {
        Debug.Log("Initiating PathingScenrioTest");
        var enemy = GameObject.Find("Enemy").GetComponent<EnemyController>();
        var timeout = 10f;
        var goal = GameObject.Find("Mr.Alien").GetComponent<Collider2D>();
        yield return new WaitForSeconds(5);

        while (timeout > 0)
        {
            if (enemy.Bounds.Intersects(goal.bounds))
            {
                Assert.Fail("Enemy reached the goal");
                break;
            }
            timeout -= Time.deltaTime;
            yield return null;
        }
        Assert.Pass("Enemy did not reach the goal");
    }

    [UnityTest]
    public IEnumerator PatrolPathValidationTest()
    {
        Debug.Log("Initiating PatrolPathValidationTest");
        yield return new WaitForSeconds(2);
        // the project groups all the patrol paths under a single game object.
        var patrolPaths = GameObject.Find("PatrolPaths").GetComponentsInChildren<PatrolPath>();

        // iterate over every patrol path and validate it.
        foreach (var path in patrolPaths)
        {
            Debug.DrawLine(path.startPosition, path.endPosition, Color.red);
            Assert.That(IsOverlapping(path), Is.False, $"{path.name} is overlapping with something.");
            Assert.That(IsOverNothing(path), Is.False, $"{path.name} is not over a continuos floor.");
            // TODO: Add more validations here.
        }

        Assert.Pass("All patrol paths are valid");
    }

    // Below are helper methods for the PatrolPathValidationTest
    bool IsOverlapping(PatrolPath path)
    {
        // path.startPosition and path.endPosition are local to the PatrolPath object, so we need to transform them to world space.
        var sp = path.transform.TransformPoint(path.startPosition);
        var ep = path.transform.TransformPoint(path.endPosition);
        // using a raycast to check if there's any collider between the start and end positions of the path.
        RaycastHit2D hit = Physics2D.Linecast(sp, ep);
        // we return true if a hit was detected, false otherwise.
        return true ? hit.collider != null : false;
    }

    bool IsOverNothing(PatrolPath path)
    {
        // same as above, we need to transform the local positions to world space.
        var sp = path.transform.TransformPoint(path.startPosition);
        var ep = path.transform.TransformPoint(path.endPosition);
        // we iterate over the path from start to end, checking if there's a gap in the floor.
        for (var current = sp; current != ep; current = Vector2.MoveTowards(current, ep, 0.1f))
        {
            // we cast a ray downwards from the current position.
            RaycastHit2D hit = Physics2D.Raycast(current, Vector2.down, 2f);
            if (hit.collider == null)
            {
                // unlike the previous method, we return true if the current raycast doesn't hit anything.
                return true;
            }
        }
        return false;
    }
}