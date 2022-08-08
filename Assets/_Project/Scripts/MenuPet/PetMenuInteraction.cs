using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetMenuInteraction : MonoBehaviour
{
    [Header("OnClickEffect")]
    [SerializeField] private float _explosionForce;
    [SerializeField] private float _fieldOfImpact;
    [SerializeField] private ParticleSystem _mouseClickRingParticle;
    [Space(10)]
    [Header("Holding")]
    [SerializeField] private float _maxMouseSpeed = 10;
    [SerializeField] private float _maxPullDistance;
    [Space(10)]
    [Header("IdleAnimation")]
    [SerializeField] private float _speed;
    [SerializeField] private float _idleAnimPower;
    [SerializeField] private List<Transform> _bonesTransforms = new();
    [SerializeField] private Transform _eyes;

    private List<(Transform transform, Vector2 startPos)> _bonesTransformsAndStartPos = new();
    private Vector2 _eyesStartPos;
    private Vector3 _mousePos, _mouseForce, _lastMousePosition, _targetStartPos;
    private GameObject _selectedObj;
    private Rigidbody2D _selectedRb;
    private bool _stopIdle = false;
    private float _counter = 0;
    private float _idleAnimDelay;
    private float _angle;

    private void Start()
    {
        foreach (Transform bone in _bonesTransforms)
        {
            _bonesTransformsAndStartPos.Add(new(bone, bone.position));
        }
        _eyesStartPos = _eyes.position;
    }

    void Update()
    {
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0;

        if (Input.GetMouseButton(0))
        {
            _counter += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _counter = 0;
        }
        //if player doesn't want to hold any bone and there's no currently selected bone to move
        if (_counter < 0.5f && Input.GetMouseButtonUp(0) && _selectedObj == null)
        {
            Explode(_mousePos);
            _mouseClickRingParticle.Play();
        }

        Collider2D targetObject = Physics2D.OverlapPoint(_mousePos);
        //if player wants to hold a bone and there's no bone selected
        if (_counter >= 0.5f && _selectedRb == null)
        {
            StartDragging(targetObject);
        }
        //if player realeases the button and was holding the bone
        else if (Input.GetMouseButtonUp(0) && _selectedRb)
        {
            StopDragging();
        }

        _idleAnimDelay -= Time.deltaTime;
        EnableAnimatorBack();
        IdleAnimation();
    }

    void FixedUpdate()
    {
        //moving any selected bone
        if (_selectedRb)
        {
            Vector3 direction = _mousePos - _targetStartPos;
            direction = Vector3.ClampMagnitude(direction, _maxPullDistance);
            Vector2 nextPos = Vector3.Lerp(_selectedRb.transform.position, _targetStartPos + direction, Time.fixedDeltaTime * 10f);
            _selectedRb.MovePosition(nextPos);
        }
    }

    private void Explode(Vector3 pos)
    {
        //creating new object with kinematic rigidbody
        GameObject explosion = new GameObject("explosion", typeof(Rigidbody2D));
        explosion.transform.position = pos;
        explosion.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        //checks if there is any collider to which we need to apply force
        Collider2D[] objects = Physics2D.OverlapCircleAll(explosion.transform.position, _fieldOfImpact);

        foreach (Collider2D obj in objects)
        {
            //checks if any of bones that will be moved by explosion is affected by idle animation
            foreach (var bone in _bonesTransformsAndStartPos)
            {
                if (obj.gameObject.name == bone.transform.gameObject.name)
                {
                    //if it is stop the animation
                    _stopIdle = true;
                }
            }
            
            //calculate the explosion direction and apply it
            Vector2 direction = obj.transform.position - explosion.transform.position;
            obj.GetComponent<Rigidbody2D>().AddForce(direction * _explosionForce);
        }
        
        //reset the delay
        _idleAnimDelay = 2f;

        //play onclick particle
        _mouseClickRingParticle.gameObject.transform.position = pos;

        Destroy(explosion);
    }

    private void StartDragging(Collider2D targetObject)
    {
        //checks if target object is not null
        if (targetObject)
        {

            //disable all target's spring joints to prevent glitching
            foreach (SpringJoint2D targetJoint in targetObject.GetComponents<SpringJoint2D>())
            {
                targetJoint.enabled = false;
            }

            //checks if target object is affected by idle animation
            foreach (var bone in _bonesTransformsAndStartPos)
            {
                if (targetObject.gameObject.name == bone.transform.gameObject.name)
                {
                    _stopIdle = true;
                }
            }
            
            //disable target's distance joint to prevent glitching
            targetObject.GetComponent<DistanceJoint2D>().enabled = false;

            _targetStartPos = targetObject.gameObject.transform.position;
            _selectedObj = targetObject.gameObject;
            _selectedRb = targetObject.transform.gameObject.GetComponent<Rigidbody2D>();

            //change target's rigidbody to kinematic to stop it from saving applied force
            _selectedRb.isKinematic = true;
        }
        if (_selectedRb)
        {
            //calculate mouse position change
            _mouseForce = (_mousePos - _lastMousePosition) / Time.deltaTime;
            _mouseForce = Vector2.ClampMagnitude(_mouseForce, _maxMouseSpeed);
            _lastMousePosition = _mousePos;
        }
    }

    private void StopDragging()
    {
        //enable all spring joints back
        foreach (SpringJoint2D targetJoint in _selectedObj.GetComponents<SpringJoint2D>())
        {
            targetJoint.enabled = true;
        }

        //if idle animation was disabled turn it on
        if (_stopIdle)
        {
            _stopIdle = false;
        }

        //enable distance joint back
        _selectedObj.GetComponent<DistanceJoint2D>().enabled = true;

        //set back the rigidbody to dynamic
        _selectedRb.isKinematic = false;

        //reset the velocity just in case
        _selectedRb.velocity = Vector2.zero;

        //move bone back to the origin
        _selectedObj.transform.position = _targetStartPos;

        _selectedObj = null;
        _selectedRb = null;
    }

    private void EnableAnimatorBack()
    {
        if (_idleAnimDelay <= 0 && !_selectedObj)
        {
            _stopIdle = false;
        }
    }

    private void IdleAnimation()
    {
        if (_stopIdle) return;
        _angle += Time.deltaTime * _speed;
        foreach (var bone in _bonesTransformsAndStartPos)
        {
            bone.transform.position = new Vector2(bone.startPos.x, bone.startPos.y + Mathf.Sin(_angle) * _idleAnimPower);
        }
        _eyes.position = new Vector2(_eyesStartPos.x, _eyesStartPos.y + Mathf.Sin(_angle) * (_idleAnimPower/2));
    }

}