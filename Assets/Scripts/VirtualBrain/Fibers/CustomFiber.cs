using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using WIDVE.Patterns;
using WIDVE.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VBTesting
{
	//[ExecuteAlways]
	public class CustomFiber : MonoBehaviour, IObserver<PathCreator>
	{
		[SerializeField]
		PathCreator _path;
		PathCreator Path => _path;
		
		//fiber settings
		[SerializeField]
        GameObject _fiberPrefab;
        GameObject FiberPrefab => _fiberPrefab;

        enum ColoringModes { Single, Striped, Random, None }

        [SerializeField]
        ColoringModes _coloringMode = ColoringModes.Single;
        ColoringModes ColoringMode => _coloringMode;

        [SerializeField]
        Material _fiberMaterial;
        Material FiberMaterial => _fiberMaterial;

        [SerializeField]
        List<Material> _fiberMaterials;
        List<Material> FiberMaterials => _fiberMaterials ?? (_fiberMaterials = new List<Material>());

		[SerializeField]
        [Range(.01f, 1f)]
        float _overallRadius = .1f;
        float OverallRadius => _overallRadius;
		
		[SerializeField]
        [Range(.01f, 1f)]
        float _fiberRadius = .01f;
        float FiberRadius => _fiberRadius;
		
		[SerializeField]
        bool _autoRadius = true;
        bool AutoRadius => _autoRadius;
		
		[SerializeField]
        [Range(1, 256)]
        int _bundleSize = 1;
        int BundleSize => _bundleSize;
		
		[SerializeField]
		[Range(0, .5f)]
		[Tooltip("Increase the automatic radius by this percentage to remove gaps between fibers")]
		float _radiusFudge = .1f;
		float RadiusFudge => _radiusFudge;
		
		[SerializeField]
		[Range(1, 20)]
		int _widthVariation = 1;
		int WidthVariation => _widthVariation;
		
		[SerializeField]
		[Range(1, 20)]
		int _windiness = 1;
		int Windiness => _windiness;
		
		[SerializeField]
		bool _locked = false;
		bool Locked => _locked;
		
		[SerializeField]
		bool _bundled = true;
		bool Bundled => _bundled;
		
		[SerializeField]
		List<float> _angleList;
		List<float> AngleList => _angleList ?? (_angleList = new List<float>());
		
		[SerializeField]
		List<float>[] _radiiList;
		//List<float>[] RadiiList => _radiiList ?? (_radiiList = new List<float>());
		
		[SerializeField]
		MeshFilter _connectorMeshStart;
		MeshFilter ConnectorMeshStart => _connectorMeshStart;
		
		[SerializeField]
		int _startTowardsIndex = 8;
		int StartTowardsIndex => _startTowardsIndex;
		
		[SerializeField]
		MeshFilter _connectorMeshEnd;
		MeshFilter ConnectorMeshEnd => _connectorMeshEnd;
		
		[SerializeField]
		int _endTowardsIndex = 8;
		int EndTowardsIndex => _endTowardsIndex;
		
		[SerializeField]
		float _curveVelocity = 2f;
		float CurveVelocity => _curveVelocity;
		
		[SerializeField]
		float _curveVelocityEnd = 2f;
		float CurveVelocityEnd => _curveVelocityEnd;
		
		[SerializeField]
		float _timeToConverge = 1f;
		float TimeToConverge => _timeToConverge;
		
		[SerializeField]
		float _timeToConvergeEnd = 1f;
		float TimeToConvergeEnd => _timeToConvergeEnd;
		
		[SerializeField]
		bool _invertNormalCheck = false;
		bool InvertNormalCheck => _invertNormalCheck;
		
		[SerializeField]
		bool _invertNormalCheckEnd = false;
		bool InvertNormalCheckEnd => _invertNormalCheckEnd;
		
		[SerializeField]
		bool _startIsSubMesh = false;
		bool StartIsSubMesh => _startIsSubMesh;
		
		[SerializeField]
		bool _endIsSubMesh = false;
		bool EndIsSubMesh => _endIsSubMesh;
		
		[SerializeField]
		GameObject _sphereContainer;
		GameObject SphereContainer => _sphereContainer;
		
		[SerializeField]
		GameObject _sphereContainerEnd;
		GameObject SphereContainerEnd => _sphereContainerEnd;

		[SerializeField]
		LayerMask _connectorMeshLayer;
		LayerMask ConnectorMeshLayer => _connectorMeshLayer;

		public void Generate()
		{
			if(!Path) return;
            if(!FiberPrefab) return;

			TubeRenderer[] fibers = GetFibers(gameObject, FiberPrefab, BundleSize);
			float actual_fiberRadius = AutoRadius ? CalculateFiberRadius(OverallRadius, fibers.Length, RadiusFudge) / Mathf.Abs(transform.lossyScale.x) :
														 FiberRadius;
			
			for(int i = 0; i < fibers.Length; i++)
			{
				if (fibers[i].points.Length != Path.path.NumPoints) 
				{
					fibers[i].points = new Vector3[Path.path.NumPoints];
				}
				
				fibers[i].radius = actual_fiberRadius;
				fibers[i].forwardAngleOffset = (i / (float)fibers.Length) * 360f;
			}
			
			//would be nice if the radii were also per fiber, instead of just the same per path point...
			float[][] radii = new float[fibers.Length][];
			for(int i = 0; i < fibers.Length; ++i)
			{
				radii[i] = new float[Path.path.NumPoints+1];
			}
			
			float[] angles = new float[fibers.Length];
			
			if(_locked && AngleList.Count == fibers.Length)
			{
				for(int j = 0; j < fibers.Length; ++j)
				{
					angles[j] = AngleList[j];
				}
			}
			else
			{
				AngleList.Clear();
				
				for(int j = 0; j < fibers.Length; ++j)
				{
					angles[j] = Random.Range(0.0f, 360f); 
					AngleList.Add(angles[j]);
				}
			}
			
			if(_locked && _radiiList != null)
			{
				for(int i = 0; i < _radiiList.Length; ++i)
				{	
					for(int j = 0; j < Path.path.NumPoints+1; ++j)
					{
						radii[i][j] = _radiiList[i][j];
					}
				}
			}
			else
			{
				int totalPoints = Path.path.NumPoints;
				int totalDeviation = totalPoints / Windiness;	//number of deviations can vary..
			
				if(!_bundled)
				{
					if(_radiiList == null || _radiiList.Length != fibers.Length)
					{
						_radiiList = new List<float>[fibers.Length];
					}
					
					for(int u = 0; u < fibers.Length; ++u)
					{
						if(_radiiList[u] == null)
						{
							_radiiList[u] = new List<float>();
						}
						
						radii[u][0] = Random.Range(OverallRadius, OverallRadius * (float)WidthVariation);

						int k = 1;
						for(int j = 0; j < Windiness; ++j)
						{
							int thisCount = 0;
							if(j+1 == Windiness)
							{
								radii[u][(j+1)*totalDeviation] = Random.Range(OverallRadius * (float)WidthVariation * 0.1f, OverallRadius * (float)WidthVariation * 0.5f);
							}
							else
							{
								radii[u][(j+1)*totalDeviation] = Random.Range(OverallRadius, OverallRadius * (float)WidthVariation);	//times 5 multiplier can vary here.
								float r = Random.Range(0f,50f);
								if(r <= 25f)
								{
									radii[u][(j+1)*totalDeviation] = -radii[u][(j+1)*totalDeviation];
								}
							}
							
							while(k < totalDeviation*(j+1))
							{
								radii[u][k] = Mathf.SmoothStep(radii[u][j*totalDeviation], radii[u][(j+1)*totalDeviation], (float)thisCount/(float)totalDeviation);
								thisCount++;
								++k;
							}
						}
						
						_radiiList[u].Clear();
						for(int j = 0; j < Path.path.NumPoints+1; ++j)
						{
							_radiiList[u].Add(radii[u][j]);
						}
					}
				}
			}
			
			SetFiberPoints(Path.path, fibers, OverallRadius, WidthVariation, Windiness, Bundled, Locked, radii, angles, ConnectorMeshStart, StartTowardsIndex, ConnectorMeshEnd, EndTowardsIndex, 
				CurveVelocity, CurveVelocityEnd, TimeToConverge, TimeToConvergeEnd, InvertNormalCheck, InvertNormalCheckEnd, StartIsSubMesh, EndIsSubMesh, SphereContainerEnd, SphereContainer, ConnectorMeshLayer);

			FiberColorFromMesh fcfm = GetComponent<FiberColorFromMesh>();
			if(fcfm) fcfm.SetColors();
			else ColorFibers(fibers, ColoringMode);
		}

		//Fiber generation methods
		static float CalculateFiberRadius(float overallRadius, int numFibers, float fudge = 0f)
		{   //https://www.mathopenref.com/polygonsides.html
			//with a little fudge for some overlap...
			if (numFibers == 1) return overallRadius;
			else return ((2 * overallRadius * (Mathf.Sin(Mathf.Deg2Rad * 180 / (float)numFibers))) / 2f) * (1 + fudge);
		}

		static TubeRenderer[] GetFibers(GameObject parentObject, Object fiberPrefab, int numFibers)
		{ 
			if(!fiberPrefab)
			{   //can't generate fibers without a prefab
				Debug.Log("Error! Please specify a fiber prefab");
				return new TubeRenderer[0];
			}
			//see how many fibers already exist
			TubeRenderer[] totalFibers = parentObject.GetComponentsInChildren<TubeRenderer>(true);
			if(totalFibers.Length < numFibers)
			{   //need to generate more fibers
				for (int i = totalFibers.Length; i < numFibers; i++)
				{
					parentObject.InstantiatePrefab(fiberPrefab);
				}
				totalFibers = parentObject.GetComponentsInChildren<TubeRenderer>(true);
			}
			//activate only the fibers we need
			for (int i = 0; i < totalFibers.Length; i++)
			{
				totalFibers[i].gameObject.SetActive(i < numFibers);
			}
			//return all the active fibers
			return parentObject.GetComponentsInChildren<TubeRenderer>(false);
		}

		static Vector3[] GetFiberPoints(Vector3 center, Vector3 direction, Vector3 normal, float overallRadius, float[] angle, int numFibers, bool bundled)
		{
			Vector3[] positions = new Vector3[numFibers];
			if (numFibers == 1)
			{   //fiber point is just the path point
				positions[0] = center;
			}
			else
			{	
				//this is where we need to change the resultant position
				//vary the overall radius per-fiber?
				//also, randomize the angle value 
				
				//arrange fiber points in a ring shape
				
				//float angle = Random.Range(0.0f, 360f);
				float angleAround = 360f / numFibers;
				direction = direction.normalized;
				normal = normal.normalized;
				for (int i = 0; i < numFibers; i++)
				{
					if(bundled)
					{
						Quaternion rotation = Quaternion.AngleAxis(angleAround * i, direction);
						positions[i] = center + (rotation * normal * overallRadius);
					}
					else
					{
						//we want this angle to vary, as well as the radius...
						Quaternion rotation = Quaternion.AngleAxis(angle[i], direction);
						positions[i] = center + (rotation * normal * overallRadius);
					}
				}
			}
			return positions;
		}
		
		static Vector3[] GetFiberPoints(Vector3 center, Vector3 direction, Vector3 normal, float overallRadius, int pathPointIndex, float[][] radii, float[] angle, int numFibers, bool bundled)
		{
			Vector3[] positions = new Vector3[numFibers];
			if (numFibers == 1)
			{   //fiber point is just the path point
				positions[0] = center;
			}
			else
			{	
				//this is where we need to change the resultant position
				//vary the overall radius per-fiber?
				//also, randomize the angle value 
				
				//arrange fiber points in a ring shape
				
				//float angle = Random.Range(0.0f, 360f);
				float angleAround = 360f / numFibers;
				direction = direction.normalized;
				normal = normal.normalized;
				for (int i = 0; i < numFibers; i++)
				{
					if(bundled)
					{
						Quaternion rotation = Quaternion.AngleAxis(angleAround * i, direction);
						positions[i] = center + (rotation * normal * overallRadius);
					}
					else
					{
						//we want this angle to vary, as well as the radius...
						Quaternion rotation = Quaternion.AngleAxis(angle[i], direction);
						positions[i] = center + (rotation * normal * radii[i][pathPointIndex]);
					}
				}
			}
			return positions;
		}

		static void SetFiberPoints(VertexPath path, TubeRenderer[] fibers, float overallRadius, int widthVariation, int windiness, bool bundled, 
			bool locked, float[][] radii, float[] angles, MeshFilter connectorMesh=null, int startTowardsIndex = 0, MeshFilter connectorMeshEnd=null, int endTowardsIndex=0, 
				float curveVelocity=2f, float curveVelocityEnd=2f, float timeToConverge=1f, float timeToConvergeEnd=1f, bool invertNormalCheck=false, bool invertNormalCheckEnd=false, 
				bool startIsSubMesh=false, bool endIsSubMesh=false, GameObject sphereEnd=null, GameObject sphere=null, int connectorMeshLayer = 0)
		{
			for(int i = 0; i < path.localPoints.Length; i++)
			{
				if(bundled)
				{
					Vector3[] points = GetFiberPoints(path.GetPoint(i),
													  path.GetTangent(i),
													  path.GetNormal(i),
													  overallRadius,
													  angles,
													  fibers.Length, bundled);
													  
					for(int j = 0; j < points.Length; j++)
					{
						fibers[j].points[i] = fibers[j].transform.InverseTransformPoint(points[j]);
					}
				}
				else
				{
					Vector3[] points = GetFiberPoints(path.GetPoint(i),
													  path.GetTangent(i),
													  path.GetNormal(i),
													  overallRadius,
													  i,
													  radii,
													  angles,
													  fibers.Length, bundled);
													  
					for(int j = 0; j < points.Length; j++)
					{
						fibers[j].points[i] = fibers[j].transform.InverseTransformPoint(points[j]);
					}
				}
			}
			
			//add an option that let's us find eligble verts based on distance from end point (x number of them)...
			
			//pre-transform the mesh vertices
			if(connectorMeshEnd != null)
			{
				int numVerts = endIsSubMesh ? connectorMeshEnd.sharedMesh.GetSubMesh(0).vertexCount : connectorMeshEnd.sharedMesh.vertices.Length;
				
				Vector3[] worldVertsEnd = new Vector3[numVerts];
				Vector3[] worldNormalsEnd = new Vector3[numVerts];
				
				int vertOffset = 0;
				if(endIsSubMesh)
				{
					vertOffset = connectorMeshEnd.sharedMesh.GetSubMesh(0).firstVertex;
				}
				
				for (int i = 0; i < numVerts; i++)
				{
					worldVertsEnd[i] = connectorMeshEnd.transform.TransformPoint(connectorMeshEnd.sharedMesh.vertices[vertOffset+i]);
					worldNormalsEnd[i] = connectorMeshEnd.transform.TransformVector(!invertNormalCheckEnd ? connectorMeshEnd.sharedMesh.normals[vertOffset+i] : -connectorMeshEnd.sharedMesh.normals[vertOffset+i]);
				}

				for(int j = 0; j < fibers.Length; ++j)
				{
					bool start = ConnectFromStart(worldVertsEnd, fibers[j].points[0], fibers[j].points[fibers[j].points.Length-1], fibers[j].transform);
					//find a vertex on the mesh that best "matches" the end point of the fiber
					int startTowardsMesh = start ? endTowardsIndex : fibers[j].points.Length-endTowardsIndex;
					//clamp to avoid index errors
					startTowardsMesh = Mathf.Clamp(startTowardsMesh, 2, fibers[j].points.Length - 1);
					
					//int meshIndex = GetClosestPointOnMesh(connectorMesh.sharedMesh, connectorMesh.transform, fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]));
					Ray r = new Ray(fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]), start ? (fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh-1]) - fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh])).normalized : 
						(fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]) - fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh-1]).normalized));
					
					List<uint> eligibleVerts = new List<uint>();
					
					for(uint k = 0; k < numVerts; ++k)
					{
						if(Vector3.Dot(worldNormalsEnd[k], r.direction) < 0f)
						{
							if(sphereEnd != null)
							{
								Bounds b = sphereEnd.GetComponent<MeshRenderer>().bounds;
								if(Vector3.Distance(b.center, worldVertsEnd[k]) < Vector3.Distance(b.center, b.max)*0.5f)
								//if(b.Contains(worldVertsEnd[k]))
								{
									eligibleVerts.Add(k);
								}
							}
							else
							{
								eligibleVerts.Add(k);
							}
						}
					}
					
					if(eligibleVerts.Count > 0)
					{

						//Debug.Log(eligibleVerts.Count);
						//int vertJump = (int)Mathf.Floor((float)eligibleVerts.Count / (float)fibers.Length);
						//int currVert = 0;

						int randVert = Random.Range(0, eligibleVerts.Count-1);
						
						Vector3 newPos = fibers[j].points[startTowardsMesh];
						
						//float fDist = 1f / (float)endTowardsIndex;
						Vector3 meshVert = worldVertsEnd[eligibleVerts[randVert]];

						//once vertex has been chosen, do another raycast from fiber point to mesh point
						//if this raycast hits a closer vertex, use that instead
						Vector3 fp = fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]);
						Ray cmRay = new Ray(fp, meshVert - fp);
						RaycastHit hitInfo;
						if(Physics.Raycast(cmRay, out hitInfo, 1000, connectorMeshLayer))
						{
							meshVert = hitInfo.point;
						}
					
						if(start)
						{
							fibers[j].points[0] = fibers[j].transform.InverseTransformPoint(meshVert);
							//Vector3 fiberSpace = fibers[j].points[0] + Vector3.Normalize(fibers[j].transform.InverseTransformVector(connectorMeshEnd.transform.TransformVector(connectorMeshEnd.sharedMesh.normals[currVert]))) * 0.2f;
							Vector3 v = Vector3.Normalize(fibers[j].points[startTowardsMesh-2] - fibers[j].points[startTowardsMesh-1])*curveVelocityEnd;
							for(int i = startTowardsMesh-1; i > 0; --i)
							{							
								newPos = Vector3.SmoothDamp(newPos, fibers[j].points[0], ref v, timeToConvergeEnd);//, Mathf.Infinity, fDist);
								fibers[j].points[i] = newPos;
								//fDist += fDist;
							}
						}
						else
						{
							fibers[j].points[fibers[j].points.Length-1] = fibers[j].transform.InverseTransformPoint(meshVert);
							Vector3 v = Vector3.Normalize(fibers[j].points[startTowardsMesh+2] - fibers[j].points[startTowardsMesh+1])*curveVelocityEnd;
							
							for(int i = startTowardsMesh+1; i < fibers[j].points.Length-1; ++i)
							{
								newPos = Vector3.SmoothDamp(newPos, fibers[j].points[fibers[j].points.Length-1], ref v, timeToConvergeEnd);//, Mathf.Infinity, fDist);
								fibers[j].points[i] = newPos;
								//fDist += fDist;
							}
						}
						
						//currVert += vertJump;
					}
				}
			}
			
			if(connectorMesh != null)
			{
				int numVerts = startIsSubMesh ? connectorMesh.sharedMesh.GetSubMesh(0).vertexCount : connectorMesh.sharedMesh.vertices.Length;
				
				Vector3[] worldVerts = new Vector3[numVerts];
				Vector3[] worldNormals = new Vector3[numVerts];
				
				int vertOffset = 0;
				if(startIsSubMesh)
				{
					vertOffset = connectorMesh.sharedMesh.GetSubMesh(0).firstVertex;
				}
				
				for (int i = 0; i < numVerts; i++)
				{
					worldVerts[i] = connectorMesh.transform.TransformPoint(connectorMesh.sharedMesh.vertices[vertOffset+i]);
					worldNormals[i] = connectorMesh.transform.TransformVector(!invertNormalCheck ? connectorMesh.sharedMesh.normals[vertOffset+i] : -connectorMesh.sharedMesh.normals[vertOffset+i]);
				}
				
				for(int j = 0; j < fibers.Length; ++j)
				{
					bool start = ConnectFromStart(worldVerts, fibers[j].points[0], fibers[j].points[fibers[0].points.Length-1], fibers[j].transform);
					//find a vertex on the mesh that best "matches" the end point of the fiber
					int startTowardsMesh = start ? startTowardsIndex : fibers[j].points.Length-startTowardsIndex;
					
					//int meshIndex = GetClosestPointOnMesh(connectorMesh.sharedMesh, connectorMesh.transform, fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]));
					Ray r = new Ray(fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]), start ? (fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh-1]) - fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh])).normalized : 
						(fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh]) - fibers[j].transform.TransformPoint(fibers[j].points[startTowardsMesh-1]).normalized));
					
					List<uint> eligibleVerts = new List<uint>();
				
					for(uint k = 0; k < numVerts; ++k)
					{
						if(Vector3.Dot(worldNormals[k], r.direction) < 0f)
						{
							if(sphere != null)
							{
								Bounds b = sphere.GetComponent<MeshRenderer>().bounds;
								if(Vector3.Distance(b.center, worldVerts[k]) < Vector3.Distance(b.center, b.max)*0.5f)
								//if(b.Contains(worldVerts[k]))
								{
									eligibleVerts.Add(k);
								}
							}
							else
							{
								eligibleVerts.Add(k);
							}
						}
					}
					
					if(eligibleVerts.Count > 0)
					{
					
						//Debug.Log(eligibleVerts.Count);
						//int vertJump = (int)Mathf.Floor((float)eligibleVerts.Count / (float)fibers.Length);
						//int currVert = 0;

						int randVert = Random.Range(0, eligibleVerts.Count-1);
						//Vector3 v = r.direction;
						Vector3 newPos = fibers[j].points[startTowardsMesh];
						
						//float fDist = 1f/(float)startTowardsIndex;
						Vector3 meshVert = worldVerts[eligibleVerts[randVert]];
					
						if(start)
						{
							fibers[j].points[0] = fibers[j].transform.InverseTransformPoint(meshVert);
							//Vector3 fiberSpace = fibers[j].points[0] + Vector3.Normalize(fibers[j].transform.InverseTransformVector(connectorMesh.transform.TransformVector(connectorMesh.sharedMesh.normals[currVert]))) * 0.2f;
							Vector3 v = Vector3.Normalize(fibers[j].points[startTowardsMesh-2] - fibers[j].points[startTowardsMesh-1])*curveVelocity;
							for(int i = startTowardsMesh-1; i > 0; --i)
							{							
								newPos = Vector3.SmoothDamp(newPos, fibers[j].points[0], ref v, timeToConverge);//, Mathf.Infinity, fDist);
								fibers[j].points[i] = newPos;
								//fDist += fDist;
							}
						}
						else
						{
							fibers[j].points[fibers[j].points.Length-1] = fibers[j].transform.InverseTransformPoint(meshVert);
							Vector3 v = Vector3.Normalize(fibers[j].points[startTowardsMesh+2] - fibers[j].points[startTowardsMesh+1])*curveVelocity;
							for(int i = startTowardsMesh+1; i < fibers[j].points.Length-1; ++i)
							{
								newPos = Vector3.SmoothDamp(newPos, fibers[j].points[fibers[j].points.Length-1], ref v, timeToConverge);//, Mathf.Infinity, fDist);
								fibers[j].points[i] = newPos;
								//fDist += fDist;
							}
						}
						
						//currVert += vertJump;
					
					}
				}
			}

			//make sure changes get saved
			for(int i = 0; i < fibers.Length; i++)
			{
				TubeRenderer fiber = fibers[i];
				fiber.Dirty();
				fiber.ForceUpdate();
#if UNITY_EDITOR
				EditorUtility.SetDirty(fiber);
#endif
			}
		}

		/*static int GetClosestPointOnMesh(Mesh mesh, Transform meshTransform, Vector3 goal)
		{	//return the index of the closest point on the mesh to the goal
			float minDistance = float.MaxValue;
			int closestPoint = -1;
			Vector3 worldVertex;
			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				worldVertex = meshTransform.TransformPoint(mesh.vertices[i]);
				float d = Vector3.Distance(worldVertex, goal);
				if (d < minDistance)
				{
					minDistance = d;
					closestPoint = i;
				}
			}
			return closestPoint;
		}*/
		
		static bool ConnectFromStart(Vector3[] worldVerts, Vector3 startPoint, Vector3 endPoint, Transform pointsTransform)
		{	//return true if fiber should connect from start, false if from end
			Vector3 worldStartPoint = pointsTransform.TransformPoint(startPoint);
			Vector3 worldEndPoint = pointsTransform.TransformPoint(endPoint);
			
			float minStartDistance = float.MaxValue;
			float minEndDistance = float.MaxValue;
			int closestPointToStart = -1;
			int closestPointToEnd = -1;
			
			for (int i = 0; i < worldVerts.Length; i++)
			{
				float d = (worldVerts[i] - worldStartPoint).sqrMagnitude;
				float dE = (worldVerts[i] - worldEndPoint).sqrMagnitude;
				if (d < minStartDistance)
				{
					minStartDistance = d;
					closestPointToStart = i;
				}
				
				if(dE < minEndDistance)
				{
					minEndDistance = dE;
					closestPointToEnd = i;
				}
			}
			
			//int closestPointToStart = GetClosestPointOnMesh(mesh, meshTransform, worldStartPoint);
			Vector3 meshStartPoint = worldVerts[closestPointToStart];
			minStartDistance = Vector3.Distance(meshStartPoint, worldStartPoint);

			//int closestPointToEnd = GetClosestPointOnMesh(mesh, meshTransform, worldEndPoint);
			Vector3 meshEndPoint = worldVerts[closestPointToEnd];
			minEndDistance = Vector3.Distance(meshEndPoint, worldEndPoint);
			
			return (minStartDistance < minEndDistance) ? true : false;
		}
	
		void ColorFibers(TubeRenderer[] fibers, ColoringModes mode)
        {
            switch(mode)
            {
                default:
				case ColoringModes.None:
					return;
                case ColoringModes.Single:
                    //apply the same color to all fibers
                    if(FiberMaterial)
                    {
                        for(int i = 0; i < fibers.Length; i++)
                        {
                            Renderer r = fibers[i].GetComponent<Renderer>();
                            if(r) r.sharedMaterial = FiberMaterial;
                        }
                    }
                    break;

                case ColoringModes.Striped:
                    //apply colors in the order they appear in the list
                    if(FiberMaterials.Count > 0)
                    {
                        for(int i = 0; i < fibers.Length; i++)
                        {
                            Renderer r = fibers[i].GetComponent<Renderer>();
                            Material stripedMaterial = FiberMaterials[i % FiberMaterials.Count];
                            if(r && stripedMaterial) r.sharedMaterial = stripedMaterial;
                        }
                    }
                    break;

                case ColoringModes.Random:
                    //apply a random color from the list to each fiber
                    if(FiberMaterials.Count > 0)
                    {
                        for(int i = 0; i < fibers.Length; i++)
                        {
                            Renderer r = fibers[i].GetComponent<Renderer>();
                            Material randomMaterial = FiberMaterials[Random.Range(0, FiberMaterials.Count)];
                            if(r && randomMaterial) r.sharedMaterial = randomMaterial;
                        }
                    }
                    break;
            }
        }

		void RemoveFibers()
        {
            //delete the existing fiber prefab instances so new ones can be created
            for(int i = transform.childCount - 1; i >= 0; i --)
            {
                GameObject g = transform.GetChild(i).gameObject;

                try
                {
                    DestroyImmediate(g);
                }
                catch(System.InvalidOperationException)
                {
                    //tried to destroy an object from the parent prefab
                    //just disable it and all of its components instead
                    TubeRenderer tr = g.GetComponent<TubeRenderer>();
                    MeshFilter mf = g.GetComponent<MeshFilter>();
                    MeshRenderer mr = g.GetComponent<MeshRenderer>();

                    if(tr) DestroyImmediate(tr);
                    if(mf) DestroyImmediate(mf);
                    if(mr) DestroyImmediate(mr);

                    g.SetActive(false);
                }
            }
        }


		public void OnNotify()
		{
			//Debug.Log("test");
			//Generate();
		}
		
#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(CustomFiber))]
	public class CustomFiberEditor : UnityEditor.Editor
	{
		 public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_locked)));
			
			//draw properties
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_path)));
			bool changed = EditorGUI.EndChangeCheck();

			//separate change check for the prefab
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberPrefab)));
			bool prefabChanged = EditorGUI.EndChangeCheck();
			changed |= prefabChanged;

			EditorGUI.BeginChangeCheck();

			SerializedProperty coloringMode = serializedObject.FindProperty(nameof(_coloringMode));
			EditorGUILayout.PropertyField(coloringMode);
			if(coloringMode.enumValueIndex == (int)ColoringModes.Single)
			{
				//show single color
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberMaterial)));
			}
			else
			{
				//show list of colors
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberMaterials)), true);
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_bundleSize)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_overallRadius)));

			//different radius modes
			SerializedProperty autoRadius = serializedObject.FindProperty(nameof(_autoRadius));
			EditorGUILayout.PropertyField(autoRadius);
			if(!autoRadius.boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fiberRadius)));
			}
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_bundled)));
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_widthVariation)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_windiness)));
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_connectorMeshStart)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_startTowardsIndex)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_curveVelocity)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_timeToConverge)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_invertNormalCheck)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_startIsSubMesh)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_sphereContainer)));
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_connectorMeshEnd)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_endTowardsIndex)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_curveVelocityEnd)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_timeToConvergeEnd)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_invertNormalCheckEnd)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_endIsSubMesh)));
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_sphereContainerEnd)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_connectorMeshLayer)));
			changed |= EditorGUI.EndChangeCheck();

			serializedObject.ApplyModifiedProperties();

			if(prefabChanged)
			{
				Debug.Log("Removing");
				//remove the old fibers so they will be remade
				foreach(CustomFiber cf in targets)
				{
					cf.RemoveFibers();
				}
			}

			if(GUILayout.Button("Generate") || changed)
			{
				Debug.Log("Generating");
				//some setting changed -> update the fibers
				foreach(CustomFiber cf in targets)
				{
					cf.Generate();
				}
			}
		}
	}

#endif
	}
}