using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderParticleSystemBehindPlayer : MonoBehaviour
{
    private SpriteRenderer _playerRenderer;
    private ParticleSystemRenderer _particleSystemRenderer;
    void Start()
    {
        _playerRenderer = GameObject.FindWithTag("Player").GetComponent<SpriteRenderer>();
        _particleSystemRenderer = GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
    }
    void Update()
    {
        _particleSystemRenderer.sortingOrder = _playerRenderer.sortingOrder - 1;
    }
}
