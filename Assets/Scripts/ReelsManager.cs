using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class ReelsManager : MonoBehaviour
{
    [Header("Video Players")]
    public VideoPlayer playerA;
    public VideoPlayer playerB;
    
    [Header("Render Textures")]
    public RenderTexture textureA;
    public RenderTexture textureB;
    
    [Header("Screen Material")]
    public Material screenMaterial; // Screen512 material
    
    [Header("Video Clips")]
    public VideoClip[] reelClips;
    public bool randomOrder = true;
    
    [Header("Timing")]
    public float reelDuration = 5f;       // How long each reel plays
    public float swipeDuration = 0.4f;    // How long the swipe animation takes
    public AnimationCurve swipeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool useA = true;              // Which player is currently active
    private float currentReelTimer = 0f;
    private bool isSwiping = false;
    private int lastClipIndex = -1;
    
    void Start()
    {
        if (reelClips == null || reelClips.Length == 0)
        {
            Debug.LogWarning("No reel clips assigned!");
            return;
        }
        
        // Start with player A playing first clip
        playerA.clip = PickNextClip();
        playerA.targetTexture = textureA;
        playerA.isLooping = false;
        playerA.Play();
        
        // Set screen material to show texture A initially
        if (screenMaterial != null)
            screenMaterial.mainTexture = textureA;
    }
    
    void Update()
    {
        if (isSwiping || reelClips == null || reelClips.Length == 0) return;
        
        currentReelTimer += Time.deltaTime;
        
        // Time to swipe to next reel?
        if (currentReelTimer >= reelDuration)
        {
            StartCoroutine(SwipeToNextReel());
        }
    }
    
    VideoClip PickNextClip()
    {
        if (reelClips.Length == 1) return reelClips[0];
        
        int index;
        if (randomOrder)
        {
            // Pick random, but not the same as last time
            do
            {
                index = Random.Range(0, reelClips.Length);
            } while (index == lastClipIndex);
        }
        else
        {
            // Sequential
            index = (lastClipIndex + 1) % reelClips.Length;
        }
        
        lastClipIndex = index;
        return reelClips[index];
    }
    
    IEnumerator SwipeToNextReel()
    {
        isSwiping = true;
        
        // Prepare the OTHER player with next clip
        VideoPlayer currentPlayer = useA ? playerA : playerB;
        VideoPlayer nextPlayer = useA ? playerB : playerA;
        RenderTexture currentTex = useA ? textureA : textureB;
        RenderTexture nextTex = useA ? textureB : textureA;
        
        nextPlayer.clip = PickNextClip();
        nextPlayer.targetTexture = nextTex;
        nextPlayer.isLooping = false;
        nextPlayer.Prepare();
        
        // Wait for next video to be ready
        while (!nextPlayer.isPrepared)
            yield return null;
        
        nextPlayer.Play();
        
        // Animate swipe: blend from current texture to next
        float elapsed = 0f;
        while (elapsed < swipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = swipeCurve.Evaluate(elapsed / swipeDuration);
            
            // Swap texture at midpoint for a "flash" transition
            // OR use texture offset for slide effect (below)
            if (screenMaterial != null)
            {
                if (t > 0.5f)
                    screenMaterial.mainTexture = nextTex;
                
                // Slide effect: offset the UV
                screenMaterial.mainTextureOffset = new Vector2(0, -t);
            }
            
            yield return null;
        }
        
        // Finalize: new texture fully visible, offset back to 0
        if (screenMaterial != null)
        {
            screenMaterial.mainTexture = nextTex;
            screenMaterial.mainTextureOffset = Vector2.zero;
        }
        
        // Stop old player to save performance
        currentPlayer.Stop();
        
        // Switch active player
        useA = !useA;
        currentReelTimer = 0f;
        isSwiping = false;
    }
    
    void OnDestroy()
    {
        // Reset material offset so it doesn't persist after play
        if (screenMaterial != null)
        {
            screenMaterial.mainTextureOffset = Vector2.zero;
        }
    }
}