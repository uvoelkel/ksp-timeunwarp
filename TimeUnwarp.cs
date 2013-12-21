using System;
using UnityEngine;

namespace udev
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class TimeUnwarp : MonoBehaviour
	{
		private bool unwarp = false;
		private double lastTime = 0.0f;
		private static double timeBuffer = 20.0f;

		void Start()
		{
			lastTime = Planetarium.GetUniversalTime();
			RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
		}

		private void drawGUI()
		{
			GUI.skin = HighLogic.Skin;
			GUIStyle style = new GUIStyle(GUI.skin.toggle); 

			GUILayout.BeginVertical();
			GUILayout.Space(60);
			unwarp = GUILayout.Toggle(unwarp, "time unwarp", style);
			GUILayout.EndVertical();
		}

		void FixedUpdate()
		{
			// unwarp disabled
			if (!unwarp) {
				lastTime = Planetarium.GetUniversalTime();
				return;
			}

			// no time warp active
			if (1 == TimeWarp.CurrentRate && 0 == TimeWarp.CurrentRateIndex) {
				lastTime = Planetarium.GetUniversalTime();
				return;
			}

			// no maneuver node
			Vessel vessel = FlightGlobals.ActiveVessel;
			if (0 == vessel.patchedConicSolver.maneuverNodes.Count) {
				lastTime = Planetarium.GetUniversalTime();
				return;
			}

			ManeuverNode node = vessel.patchedConicSolver.maneuverNodes[0];
			double ut = Planetarium.GetUniversalTime();

			// node already passed
			if (ut > node.UT) {
				lastTime = ut;
				return;
			}

			double skip = ut - lastTime;
			double deltaV = (Math.Abs(node.DeltaV.x) + Math.Abs(node.DeltaV.y) + Math.Abs(node.DeltaV.z)) / 50.0f;

			if (ut + (skip * 3) + timeBuffer + deltaV >= node.UT) {

				int index = TimeWarp.CurrentRateIndex;
				if (0 < index) {
					index--;
				}

				Debug.Log ("TimeUnwarp: slowing down from " + TimeWarp.CurrentRate + "x (" + TimeWarp.CurrentRateIndex + ") to " + index);
				TimeWarp.SetRate(index, true);
			}
			lastTime = ut;
		}
	}
}

