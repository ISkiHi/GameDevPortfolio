/// <summary>
/// Camera Manager
/// 
/// Controls camera behaviour during gameplay. Responsible for adjusting
/// the camera's position and orthographic size based on screen aspect
/// ratio, safe area, and player position.
/// 
/// The system ensures the level remains visible across different
/// device screen sizes while preventing the camera from moving
/// outside level boundaries.
/// 
/// Responsibilities:
/// - Adjust camera size for different screen aspect ratios
/// - Position the camera relative to the player
/// - Respect level boundaries and grid size
/// - Handle camera movement updates during gameplay
/// </summary>

namespace DontWitchMeNow
{
    public class CameraManager : MonoBehaviour 
    {
        [SerializeField] private float baseOrthographicSize = 22f;
        [SerializeField] private Vector2Int limitsX = new Vector2Int(4, 6);     // Left, right
        [SerializeField] private Vector2Int limitsY = new Vector2Int(3, 4);     // Up, down
        
        private Vector3 cameraPosition;
        private Vector2Int fixedCamera = Vector2Int.zero;
        private bool canMoveCamera = false;
        private bool isCameraMoving = false;
        
        private void LateUpdate()
        {
            if (canMoveCamera && isCameraMoving)
                MoveCamera();
        }

        public void AdjustCameraSize()
        {
            float currentAspectRatio = (float)Screen.width / Screen.height;
            Camera.main.orthographicSize = baseOrthographicSize * (Camera.main.aspect / currentAspectRatio);

            AdjustCameraPosition();
        }

        public void AdjustCameraPosition()
        {
            float safeAreaRatio = Screen.safeArea.width / Screen.safeArea.height;
            Vector2 offset;
            Vector3 startPos = new Vector3(GameManager.Instance.player.transform.position.x, GameManager.Instance.player.transform.position.y, transform.position.z);

            /* minRatio & maxRatio checks the safe area ratio width / height and sets camera position based on this
            * camera is fixed to the center of the level if column length is > than maxColumn or row length is > than maxRow
            * if not fixed, the camera offsets based on positionOffset
            * limits determines when the camera should start and stop moving based on how close the player is to the perimeter*/ 
            var aspectRatioSettings = new List<(float minRatio, float maxRatio, int maxColumn, int maxRow, Vector2Int limits)>
            {
                (0f, 1.4f, 11, 9, new Vector2Int(2, 4)),
                (1.4f, 1.6f, 12, 9, new Vector2Int(2, 4)),
                (1.6f, 1.8f, 14, 9, new Vector2Int(3, 4)),
                (1.8f, 2f, 16, 9, new Vector2Int(3, 4)),
                (2f, 2.2f, 17, 9, new Vector2Int(3, 4)),
                (2.2f, 2.4f, 19, 8, new Vector2Int(4, 4)),
                (2.4f, 2.6f, 21, 8, new Vector2Int(4, 4)),
                (2.6f, 4.0f, 22, 8, new Vector2Int(5, 4))
            };

            cameraPosition = transform.position;
            cameraPosition.z = startPos.z;

            foreach (var setting in aspectRatioSettings)
            {
                if (safeAreaRatio >= setting.minRatio && safeAreaRatio < setting.maxRatio)
                {
                    fixedCamera = Vector2Int.zero;
                    offset = FindOffsets();

                    if (GameManager.Instance.gridManager.gridTiles.Length > setting.maxColumn)
                        cameraPosition.x = startPos.x + offset.x;
                    else
                    {
                        fixedCamera.x = 1;
                        offset.x = 4.6f;
                        cameraPosition.x = offset.x * (GameManager.Instance.gridManager.gridTiles.Length / 2);
                    }

                    if (GameManager.Instance.gridManager.gridTiles[0].Length >= setting.maxRow)
                        cameraPosition.y = startPos.y + offset.y;
                    else
                    {
                        fixedCamera.y = 1;
                        offset.y = -4.4f;
                        cameraPosition.y = offset.y * (GameManager.Instance.gridManager.gridTiles[0].Length / 2);
                    }

                    break;
                }
            }

            transform.position = cameraPosition;
        }

        private Vector2 FindOffsets()
        {
            Vector2 offset = Vector2.zero;

            if (GameManager.Instance.player.gridPosition.x - limitsX.x < 0)
                offset.x = GameManager.Instance.player.gridPosition.x - limitsX.x;
            else if (GameManager.Instance.player.gridPosition.x + limitsX.y > GameManager.Instance.gridManager.gridTiles.Length)
                offset.x = GameManager.Instance.player.gridPosition.x + limitsX.y - GameManager.Instance.gridManager.gridTiles.Length;

            if (GameManager.Instance.player.gridPosition.y - limitsY.x < 0)
                offset.y = GameManager.Instance.player.gridPosition.y - limitsY.x;
            else if (GameManager.Instance.player.gridPosition.y + limitsY.y > GameManager.Instance.gridManager.gridTiles[0].Length)
                offset.y = GameManager.Instance.player.gridPosition.y + limitsY.y - GameManager.Instance.gridManager.gridTiles[0].Length;

            offset *= GameManager.Instance.gridManager.tileDistance.x;
            offset.x *= -1;

            return offset;
        }
    }
}