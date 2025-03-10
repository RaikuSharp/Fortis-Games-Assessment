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
        SceneManager.LoadScene("SandBox");
    }

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
        var patrolPaths = GameObject.Find("PatrolPaths").GetComponentsInChildren<PatrolPath>();
        var levelCollision = GameObject.Find("Level").GetComponent<TilemapCollider2D>();

        foreach (var path in patrolPaths)
        {
            Debug.DrawLine(path.startPosition, path.endPosition, Color.red);
            Assert.That(IsOverlapping(path), Is.False, $"{path.name} is overlapping with something.");
            Assert.That(IsOverNothing(path), Is.False, $"{path.name} is not over a continuos floor.");
        }

        Assert.Pass("All patrol paths are valid");
    }

    bool IsOverlapping(PatrolPath path)
    {
        var sp = path.transform.TransformPoint(path.startPosition);
        var ep = path.transform.TransformPoint(path.endPosition);
        RaycastHit2D hit = Physics2D.Linecast(sp, ep);
        return true ? hit.collider != null : false;
    }

    bool IsOverNothing(PatrolPath path)
    {
        var sp = path.transform.TransformPoint(path.startPosition);
        var ep = path.transform.TransformPoint(path.endPosition);
        for (var current = sp; current != ep; current = Vector2.MoveTowards(current, ep, 0.1f))
        {
            RaycastHit2D hit = Physics2D.Raycast(current, Vector2.down, 2f);
            if (hit.collider == null)
            {
                return true;
            }
        }
        return false;
    }
}