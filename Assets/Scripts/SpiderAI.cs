using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class SpiderAI : MonoBehaviour
    {
        private NavMeshAgent agent;
        public GameObject target;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();    
        }

        private void Update()
        {
            MoveToTarget();
        }

        void MoveToTarget()
        {
            agent.SetDestination(target.transform.position);
        }
    }
}
