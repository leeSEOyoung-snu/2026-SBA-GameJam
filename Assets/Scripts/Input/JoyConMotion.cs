using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

// Joy2Win(motion_udp.py)이 보내는 자이로/가속도 UDP를 받아서 노출한다.
// 빈 GameObject에 붙이면 됨. 다른 스크립트에서:
//   var g = JoyConMotion.Instance.GetGyro(1, 'R');   // 플레이어1 오른쪽 조이콘 각속도(deg/s)
//   var a = JoyConMotion.Instance.GetAccel(1, 'R');  // 가속도(g)
public class JoyConMotion : MonoBehaviour
{
    public int port = 9870; // motion_udp.py의 _UDP_PORT와 동일

    public static JoyConMotion Instance { get; private set; }

    [Serializable]
    private class MotionPacket
    {
        public int p;        // 플레이어 번호
        public string side;  // "L" / "R"
        public float ax, ay, az; // 가속도 (g)
        public float gx, gy, gz; // 자이로 (deg/s)
        public long t;
    }

    public struct Motion
    {
        public Vector3 accel; // g
        public Vector3 gyro;  // deg/s
        public long timestamp;
    }

    private UdpClient _udp;
    private Thread _thread;
    private volatile bool _running;
    private readonly ConcurrentQueue<string> _incoming = new ConcurrentQueue<string>();
    private readonly Dictionary<string, Motion> _latest = new Dictionary<string, Motion>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartListening();
    }

    void StartListening()
    {
        try
        {
            _udp = new UdpClient(port);
            _running = true;
            _thread = new Thread(ReceiveLoop) { IsBackground = true };
            _thread.Start();
            Debug.Log($"[JoyConMotion] UDP 수신 시작 (포트 {port})");
        }
        catch (Exception e)
        {
            Debug.LogError($"[JoyConMotion] 수신 시작 실패: {e.Message}");
        }
    }

    void ReceiveLoop()
    {
        var remote = new IPEndPoint(IPAddress.Any, 0);
        while (_running)
        {
            try
            {
                byte[] bytes = _udp.Receive(ref remote);
                _incoming.Enqueue(System.Text.Encoding.UTF8.GetString(bytes));
            }
            catch (SocketException) { /* 종료 시 Close로 인한 예외 무시 */ }
            catch (ObjectDisposedException) { break; }
            catch (Exception e) { Debug.LogWarning($"[JoyConMotion] {e.Message}"); }
        }
    }

    void Update()
    {
        // JsonUtility는 메인 스레드에서만 파싱 (스레드 안전 보장 X)
        while (_incoming.TryDequeue(out string json))
        {
            MotionPacket pkt;
            try { pkt = JsonUtility.FromJson<MotionPacket>(json); }
            catch { continue; }
            if (pkt == null || string.IsNullOrEmpty(pkt.side)) continue;

            string key = pkt.p + "_" + pkt.side.ToUpper();
            bool isNew = !_latest.ContainsKey(key);
            _latest[key] = new Motion
            {
                accel = new Vector3(pkt.ax, pkt.ay, pkt.az),
                gyro = new Vector3(pkt.gx, pkt.gy, pkt.gz),
                timestamp = pkt.t
            };
            if (isNew)
                Debug.Log($"[JoyConMotion] 새 키 등록: {key}");
        }
    }

    // ---- 조회 API ----
    public bool TryGet(int player, char side, out Motion m)
        => _latest.TryGetValue(player + "_" + char.ToUpper(side), out m);

    public Vector3 GetGyro(int player, char side)
        => TryGet(player, side, out var m) ? m.gyro : Vector3.zero;

    public Vector3 GetAccel(int player, char side)
        => TryGet(player, side, out var m) ? m.accel : Vector3.zero;

    void OnDestroy()
    {
        _running = false;
        try { _udp?.Close(); } catch { }
        try { _thread?.Join(200); } catch { }
    }
}
