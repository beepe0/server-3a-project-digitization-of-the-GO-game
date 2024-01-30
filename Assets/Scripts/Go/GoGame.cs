using Network.Connection;
using Network.Connection.Player;
using Network.UnityServer;
using Network.UnityTools;
using UnityEngine;

namespace Go
{
    [RequireComponent(typeof(GoSettings))]
    [RequireComponent(typeof(GoRules))]
    [RequireComponent(typeof(GoBoard))]
    public class GoGame : MonoBehaviour
    {
        [SerializeField] private Connection _conn;
        
        [SerializeField] private GoSettings _goSettings;
        [SerializeField] private GoRules _goRules;
        [SerializeField] private GoBoard _goBoard;
        
        public Connection Conn => _conn;
        public GoSettings Settings => _goSettings;
        public GoRules Rules => _goRules;
        public GoBoard Board => _goBoard;

        private void Awake()
        {
            _conn = GameObject.FindWithTag("Network").GetComponent<Connection>();
            _goSettings = gameObject.GetComponent<GoSettings>();
            _goRules = gameObject.GetComponent<GoRules>();
            _goBoard = gameObject.GetComponent<GoBoard>();
            
            _goRules.GoGame = this;
            _conn.RulesHandler.AddRule((ushort)Connection.PacketType.PawnOpen, _goRules.PawnInitialization);
            _conn.RulesHandler.AddRule((ushort)Connection.PacketType.PawnPass, _goRules.PawnPass);
        }
        public void InitializingGame(GoGame goGame)
        {
            gameObject.transform.localScale = new Vector3((_goSettings.boardSize.x - 1) / _goSettings.cellsSize, 1, (_goSettings.boardSize.y - 1) / _goSettings.cellsSize);
            _goSettings.boardMaterial.mainTextureScale = new Vector2((_goSettings.boardSize.x - 1), (_goSettings.boardSize.y - 1));
            _goSettings.pawnsSize = (20 / _goSettings.cellsSize) / _goSettings.cellsCoefSize;
            
            _goBoard.offset = new Vector2(gameObject.transform.localScale.x / 2, -gameObject.transform.localScale.z / 2);
            _goBoard.pawnOffset = new Vector2(_goBoard.offset.x, -_goBoard.offset.y);
            _goBoard.pawns = new GoPawn[_goSettings.boardSize.x * _goSettings.boardSize.y];

            for (int x = 0; x < _goSettings.boardSize.x; x++)
            {
                for (int y = 0; y > -_goSettings.boardSize.y; y--)
                {
                    short convertMatrixToLine = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x, y));
                    
                    Vector3 newPos = new Vector3(x / _goSettings.cellsSize - (_goBoard.pawnOffset.x), 0.5f, y / _goSettings.cellsSize + (_goBoard.pawnOffset.y));
                    GameObject pawnObject = Instantiate(_goSettings.prefabPawnAB, newPos, Quaternion.identity, gameObject.transform);
                    GoPawn node = new GoPawn(goGame, (ushort)convertMatrixToLine, pawnObject);

                    pawnObject.SetActive(false);
                    pawnObject.name = $"xyz: {newPos}";
                    pawnObject.transform.SetParent(gameObject.transform);
                    
                    for(ushort i = 0; i < 4; i++)
                    {
                        short mtl = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x + GoPawn.OffsetNeighbours[i].x, y + GoPawn.OffsetNeighbours[i].y));
                        node.Neighbours[i] = mtl >= 0 && mtl < _goBoard.pawns.Length ? _goBoard.pawns[mtl] : null;
                    }
                    
                    _goBoard.pawns[convertMatrixToLine] = node;
                }
            }
            
            for (int x = 0; x < _goSettings.boardSize.x; x++)
            {
                for (int y = 0; y > -_goSettings.boardSize.y; y--)
                {
                    int convertMatrixToLine = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x, y));
                    
                    for(ushort i = 0; i < 4; i++)
                    {
                        short mtl = GoTools.ConvertMatrixToLine(_goSettings.boardSize, new Vector2(x + GoPawn.OffsetNeighbours[i].x, y + GoPawn.OffsetNeighbours[i].y));
                        _goBoard.pawns[convertMatrixToLine].Neighbours[i] = mtl >= 0 && mtl < _goBoard.pawns.Length ? _goBoard.pawns[mtl] : null;
                    }
                }
            }
        }
        public void PawnOpen(ushort clientId, GoPawn goPawn)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.PawnOpen);
            packet.Write(goPawn.index);
            packet.Write((byte)goPawn.pawnType);
            _conn.DataHandler.SendDataToAllTcp(clientId, packet);
        }
        public void PawnClose(ushort clientId, GoPawn goPawn)
        {
            UNetworkIOPacket packet = new UNetworkIOPacket((ushort)Connection.PacketType.PawnClose);
            packet.Write(goPawn.index);
            _conn.DataHandler.SendDataToAllTcp(clientId, packet);
        }
    }
}
