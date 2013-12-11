// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Exit Games GmbH">
//   Exit Games GmbH, 2012
// </copyright>
// <summary>
//  Creates and controls the behaviour of the interface elements
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Client.DemoParticle;
using System.Threading;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DemoGUI : MonoBehaviour 
{

    // Main menu parameters
    public static bool ShowUserInfo { get; private set; }
    public static bool ShowMainMenu { get; private set; }
    private static int DefaultButtonWidth { get; set; }
    private static int DefaultButtonHeight { get; set; }
    private static int IntervalBetweenButtons { get; set; }

    private static bool AutomoveEnabled { get; set; }
    private static bool MultipleRoomsEnabled { get; set; }
    private static bool IsGameStarted { get; set; }

    // Logic of the game
    private Logic logic;

    // Scale of playground grid
    public static int playgroundScale { get; private set; }
    private float scaleRatio;

    // Initialization code 
    public void Init (string serverAddress, string appId, string gameVersion) {

        ShowMainMenu = true;
        ShowUserInfo = true;

        AutomoveEnabled = true;

        #if UNITY_ANDROID
            DefaultButtonWidth = 100;
            DefaultButtonHeight = 60;
        #else
            DefaultButtonWidth = 40;
            DefaultButtonHeight = 40;
        #endif

        IntervalBetweenButtons = 10;

        // Start the logic of the game
        logic = new Logic();
        logic.StartGame(serverAddress, appId, gameVersion);

        DemoGUI.playgroundScale = (int) GameObject.Find("Playground").transform.localScale.x;  
	}

    
	// Update is called once per frame
	void Update () {

        // Update logic and render players
        if (logic != null)
        {
            logic.UpdateLocal();

            if (logic.LocalPlayerJoined())
            {
                RenderPlayers();
            }
        }

        // Show or hide main menu when 'Tab' is pressed
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ShowMainMenu = !ShowMainMenu;
        }

        // Exit the application when 'Back' button is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}

    // Draw the GUI
    void OnGUI()
    {
        if (logic != null && logic.LocalPlayerJoined())
        {
            if (ShowMainMenu)
            {
                #region menu buttons
                int i = 0;
                int buttonSpace = IntervalBetweenButtons + DefaultButtonWidth;
                // Automove button
                if (AutomoveEnabled)
                {
                    if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("automove off") as Texture))
                    {
                        AutomoveEnabled = !AutomoveEnabled;
                        this.logic.localPlayer.MoveInterval.IsEnabled = !this.logic.localPlayer.MoveInterval.IsEnabled;
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("automove on") as Texture))
                    {
                        AutomoveEnabled = !AutomoveEnabled;
                        this.logic.localPlayer.MoveInterval.IsEnabled = this.logic.localPlayer.MoveInterval.IsEnabled;
                    }
                }

                // Interest management button
                if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("interest management icon") as Texture))
                {
                    logic.SwitchUseInterestGroups();
                    SetPlaygroundTexture();
                }

                // Change color button
                if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("change color icon") as Texture))
                {
                    this.logic.localPlayer.ChangeLocalPlayercolor();
                }

                // Show user information button
                if (ShowUserInfo)
                {
                    if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("client info icon") as Texture))
                    {
                        ShowUserInfo = !ShowUserInfo;
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("client info icon") as Texture))
                    {
                        ShowUserInfo = !ShowUserInfo;
                    }
                }

                // Add client button
                if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("add client icon") as Texture))
                {
                    logic.AddClient();
                }

                // Remove client button
                if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("remove client icon") as Texture))
                {
                    logic.RemoveClient();
                }

                // Change grid size button
                if (GUI.Button(new Rect(buttonSpace * (i++) + IntervalBetweenButtons, 10, DefaultButtonWidth, DefaultButtonHeight), Resources.Load("grid icon") as Texture))
                {
                    logic.ChangeGridSize();
                    SetPlaygroundTexture();
                }
                #endregion
            }
        }
        else
        {
            if (logic != null)
            {
                GUI.Label(new Rect(10, 10, 300, 50), this.logic.localPlayer.State.ToString());
            }
        }

        if (DemoGUI.ShowUserInfo && logic.LocalPlayerJoined())
        {
            // Get 2D coordinates from 3D coordinates of the client

            scaleRatio = DemoGUI.playgroundScale / this.logic.localPlayer.GridSize;
            Vector3 localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Bold;

            foreach (ParticlePlayer p in logic.localPlayer.LocalRoom.Players.Values)
            {
                Vector3 posVector = Camera.main.WorldToScreenPoint(new Vector3(p.PosX * localScale.x + localScale.x / 2, scaleRatio / 2, p.PosY * localScale.y + localScale.y / 2));

                if (p.IsLocal)
                {
                    labelStyle.fontStyle = FontStyle.Bold;
                    labelStyle.normal.textColor = Color.black;
                }
                else
                {
                    labelStyle.fontStyle = FontStyle.Normal;
                    labelStyle.normal.textColor = Color.white;
                }
                // Output the client's name
                GUI.Label(new Rect(posVector.x, Screen.height - posVector.y, 100, 20), p.Name, labelStyle);
            }
        } 

    }

    // Set the playground texture
    // Current texture depends on interest management setting 
    public void SetPlaygroundTexture()
    {
        GameObject playground = GameObject.Find("Playground");

        Material playgroundMaterial;
        Texture texture;
        float textureScale;

        Material playgroundGridMaterial;
        Texture gridTexture;
        float gridTextureScale;

        Material[] materials;

        materials = playground.renderer.materials;
        
        playgroundMaterial = new Material(Shader.Find("Diffuse"));

        playgroundGridMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        gridTexture = Resources.Load("interest management grid") as Texture;
        gridTextureScale = logic.localPlayer.GridSize;
        playgroundGridMaterial.mainTexture = gridTexture;

        if (logic.localPlayer.UseInterestGroups)
        {
            texture = Resources.Load("interest groups enabled texture") as Texture;
            textureScale = 1.0f;

            materials[0] = playgroundMaterial;
            materials[0].mainTexture = texture;
            materials[0].mainTextureScale = new Vector2(textureScale, textureScale);
            
        }
        else
        {
            texture = Resources.Load("interest groups disabled texture") as Texture;
            textureScale = this.logic.localPlayer.GridSize;
            
            materials[0] = playgroundMaterial;
            materials[0].mainTexture = texture;
            materials[0].mainTextureScale = new Vector2(textureScale, textureScale);
        }

        materials[1] = playgroundGridMaterial;
        materials[1].mainTexture = gridTexture;
        materials[1].mainTextureScale = new Vector2(gridTextureScale, gridTextureScale);

        texture.wrapMode = TextureWrapMode.Repeat;
        gridTexture.wrapMode = TextureWrapMode.Repeat;

        playground.renderer.materials = materials;
    }

    /// <summary>
    /// Convert integer value to Color
    /// </summary>
    public static Color IntToColor(int colorValue)
    {     
        float r = (byte)(colorValue >> 16) / 255.0f; 
        float g = (byte)(colorValue >> 8) / 255.0f; 
        float b = (byte)colorValue / 255.0f;

        return new Color(r, g, b);
    }

    /// <summary>
    /// Render cubes onto the scene
    /// </summary>
    void RenderPlayers()
    {
        float newscale = DemoGUI.playgroundScale / this.logic.localPlayer.GridSize;

        if (newscale != scaleRatio)
        {
            scaleRatio = newscale;
            SetPlaygroundTexture();
        }
        Vector3 localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);

        lock (logic.localPlayer)
        {
            foreach (ParticlePlayer p in logic.localPlayer.LocalRoom.Players.Values)
            {
                foreach (GameObject cube in logic.cubes)
                {
                    if (cube.name == p.Name)
                    {
                        float alpha = 1.0f;
                        if (!p.IsLocal && p.UpdateAge > 500)
                        {
                            cube.renderer.material.shader = Shader.Find("Transparent/Diffuse");
                            alpha = (p.UpdateAge > 1000) ? 0.3f : 0.8f;
                        }
                        cube.transform.localScale = localScale;

                        Color cubeColor = DemoGUI.IntToColor(p.Color);
                        cube.renderer.material.color = new Color(cubeColor.r, cubeColor.g, cubeColor.b, alpha);
                        cube.transform.position = new Vector3(p.PosX * localScale.x + localScale.x / 2, scaleRatio / 2, p.PosY * localScale.y + localScale.y / 2);
                        break;
                    }
                }
            }
        }
    }
}