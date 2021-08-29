using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSpawn : MonoBehaviour
{
    [SerializeField]
    private GameObject linePartPrefab, parentObject;

    [SerializeField]
    [Range(1, 1000)]
    private int length = 1;

    [SerializeField]
    private float linePartDistance = 0.21f;

    [SerializeField]
    private bool reset, spawn, snapFirst, snapLast;


    // Update is called once per frame
    void Update()
    {
        if(reset)
        {
            foreach (GameObject linepart in GameObject.FindGameObjectsWithTag("LinePart"))
                Destroy(linepart);

            reset = false;
        }

        if(spawn)
        {
            Spawn();

            spawn = false;
        }
    }

    public void Spawn()
    {
        int count = (int)(length / linePartDistance);

        for(int i=0; i<count; i++)
        {
            Vector3 partPosition = new Vector3(transform.position.x, transform.position.y + linePartDistance * (i + 1), transform.position.z);
            GameObject part;
            part = Instantiate(linePartPrefab, partPosition, Quaternion.identity, parentObject.transform);
            //part.transform.eulerAngles = new Vector3(180f, 0f, 0f);
            part.name = parentObject.transform.childCount.ToString();

            if (i == 0)
            {
                Destroy(part.GetComponent<CharacterJoint>());
                if(snapFirst)
                {
                    part.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }
            }
            else
            {
                part.GetComponent<CharacterJoint>().connectedBody = parentObject.transform.Find
                    ((parentObject.transform.childCount - 1).ToString()).GetComponent<Rigidbody>();
            }
        }

        if(snapLast)
        {
            parentObject.transform.Find((parentObject.transform.childCount).ToString()).GetComponent<Rigidbody>()
                .constraints = RigidbodyConstraints.FreezeAll;
        }

    }
}
