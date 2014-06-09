using System;
using UnityEngine;

namespace udev
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class TimeUnwarp : MonoBehaviour
	{
		private bool unwarp = false;
		private string buffer = "20";
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
			buffer = GUILayout.TextField(buffer);
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
					

			double ut = Planetarium.GetUniversalTime();
			Vessel vessel = FlightGlobals.ActiveVessel;

			// soi
			double soi = vessel.orbit.UTsoi - ut;
			if (0 > soi) {
				soi = 0;
			}

			// maneuver node
			double node = 0;
			foreach (ManeuverNode mn in vessel.patchedConicSolver.maneuverNodes) {
				if (ut < mn.UT) {
					node = mn.UT - ut;
					break;
				}
			}

			// no soi and no maneuver node
			if (0 == soi && 0 == node) {
				lastTime = Planetarium.GetUniversalTime();
				return;
			}

			int timeIndex = TimeWarp.CurrentRateIndex;
			double timeSkipped = ut - lastTime;
			double timeLeft = node;
			if (0 == node || (0 < soi && soi < node)) {
				timeLeft = soi;
			}

			try {
				timeBuffer = double.Parse(buffer);
			}
			catch(Exception) {
				timeBuffer = 20.0f;
			}

			//Debug.Log ("TimeUnwarp: skipped: " + timeSkipped + " left: " + timeLeft + " index: " + timeIndex + " node: " + node + " soi: " + soi);

			if ((timeSkipped * (timeIndex + 1)) >= (timeLeft - timeBuffer)) {

				if (0 < timeIndex) {
					timeIndex--;
				}

				Debug.Log ("TimeUnwarp: slowing down from " + TimeWarp.CurrentRate + "x (" + TimeWarp.CurrentRateIndex + ") to " + timeIndex);
				TimeWarp.SetRate(timeIndex, true);
			}
			lastTime = ut;
		}
	}
}

