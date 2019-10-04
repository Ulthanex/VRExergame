using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*-----------------------------------------*///By combining meshes into a single Mesh, there are fewer
//Combines meshes for performance increases////Game objects to draw which each present some overhead 
/*-----------------------------------------*/// aswell as keeping object hierarchy small. one concern
//though is you may lose a lot of the upside in performance if working with a lot of materials

[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildren : MonoBehaviour
{

    private Dictionary<string, List<CombineInstance>> combines;
    private Dictionary<string, Material> namedMaterials;

    void Start()
    {
        Matrix4x4 myTransform = transform.worldToLocalMatrix;
        combines = new Dictionary<string, List<CombineInstance>>();
        namedMaterials = new Dictionary<string, Material>();

		//For each mesh renderer within all children attached to this gameobject
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            //Grab each material in shared materials for the current mesh renderer
			foreach (var material in meshRenderer.sharedMaterials)
                if (material != null && !combines.ContainsKey(material.name))
                {
              
					combines.Add(material.name, new List<CombineInstance>());
                    namedMaterials.Add(material.name, material);
                }
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (var filter in meshFilters)
        {
            if (filter.sharedMesh == null)
                continue;
            var filterRenderer = filter.GetComponent<Renderer>();
            if (filterRenderer.sharedMaterial == null)
                continue;
            if (filterRenderer.sharedMaterials.Length > 1)
                continue;
            CombineInstance ci = new CombineInstance
            {
                mesh = filter.sharedMesh,
                transform = myTransform * filter.transform.localToWorldMatrix
            };
            combines[filterRenderer.sharedMaterial.name].Add(ci);

            Destroy(filterRenderer);
        }

        foreach (Material m in namedMaterials.Values)
        {
            var go = new GameObject("Combined mesh");
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var filter = go.AddComponent<MeshFilter>();
            filter.mesh.CombineMeshes(combines[m.name].ToArray(), true, true);

            var arenderer = go.AddComponent<MeshRenderer>();
            arenderer.material = m;
        }

        turnOnLightsAfterMeshCombine();

    }

    void turnOnLightsAfterMeshCombine()
    {
        //Iterating through and turning on lights if it is night time.
        GameObject controller = GameObject.Find("IntensityController");
        if (!controller.GetComponent<IntensityController>().day)
        {
            GameObject lamps = transform.Find("Main/Lamps").gameObject;
            foreach (Transform light in lamps.transform)
            {
                light.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
}