using UnityEngine;
using UnityEngine.AI;

namespace WaveSurvival.Enemies
{
    /// <summary>Thin wrapper over NavMeshAgent. Only knows how to walk somewhere.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float repathInterval = 0.25f;

        private NavMeshAgent _agent;
        private float _nextRepath;

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        public void Configure(float speed, float stoppingDistance)
        {
            _agent.speed = speed;
            _agent.stoppingDistance = stoppingDistance * 0.8f;
        }

        public void Chase(Vector3 destination)
        {
            if (!_agent.isOnNavMesh || Time.time < _nextRepath) return;
            _nextRepath = Time.time + repathInterval;
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
        }
    }
}
