using UnityEngine;
using System;

// 
public class CameraController : MonoBehaviour
{
		// orbital camera
		protected Vector3 lookAt = new Vector3 (0, 0, 0);
		protected float radius = 12.0f;
		protected float angle_h = 0.0f;
		protected float angle_v = -30.0f;
		protected Plane ground_plane = new Plane (new Vector3 (0, 1, 0), new Vector3 (0, 0, 0));
		protected bool drag = false;
		protected Vector3 drag_origin;

		//
		void Start ()
		{
				// do camera placement
				OnGUI ();
		}

		//
		void Awake ()
		{

		}

		//
		void OnGUI ()
		{				
				// process scroll wheel
				Event e = Event.current;
				if (e == null)
						return;

				if (e.type == EventType.ScrollWheel) {
						float mscroll = e.delta.y;
						if (mscroll < 0) {
								radius *= 0.95f;
								radius -= 0.001f;
								if (radius < 0.01f)
										radius = 0.01f;
						} else if (mscroll > 0) {
								radius *= 1.05f;
								radius += 0.001f;
								if (radius > 10.0f)
										radius = 10.0f;
						}
				} else if (e.type == EventType.MouseDrag) {
						// camera keyboard panning
						if (e.button == 1) {
								angle_h += e.delta.x;
								angle_v += e.delta.y;
						}
				}
		}

		//
		void Update ()
		{
				if (Input.GetMouseButtonUp (2)) {
						drag = false;
				}

				if (Input.anyKey) {
						// mouse drag for changing orbital center
						Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
						float d = 0f;
						ground_plane.Raycast (ray, out d);
						if (d != 0f) {
								Vector3 p = ray.origin + ray.direction * d;
								p.y = 0;
								if (Input.GetMouseButtonDown (2)) {
										drag = true;
										drag_origin = p;
								} else if (Input.GetMouseButton (2) && drag) {
										lookAt = lookAt + (drag_origin - p);
								} else {
										drag = false;
								}
						}

						// camera mouse panning
						float v = Input.GetAxis ("Vertical");
						float h = Input.GetAxis ("Horizontal");
						angle_h += -h;
						angle_v += -v;
				}

				// update camera transform
				Quaternion q = Quaternion.Euler (angle_v, angle_h, 0);
				transform.position = lookAt + (q * new Vector3 (0, 0, radius));
				transform.LookAt (lookAt, Vector3.up);
		}

		//
		void OnDestroy ()
		{

		}

}