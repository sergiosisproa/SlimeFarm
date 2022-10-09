using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask collisionObjects;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask battleGround;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask portalLayer;

    public static GameLayers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask CollisionLayer
    {
        get => collisionObjects;
    }
    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }
    public LayerMask BattleGroundLayer
    {
        get => battleGround;
    }
    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }
    public LayerMask FovLayer
    {
        get => fovLayer;
    }
    public LayerMask PortalLayer
    {
        get => portalLayer;
    }
    public LayerMask TriggerableLayers
    {
        get => battleGround | fovLayer | portalLayer;
    }
}
