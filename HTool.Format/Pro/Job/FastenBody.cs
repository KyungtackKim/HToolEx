using System.Buffers.Binary;
using System.Text;
using HTool.Core.Type.Pro;

namespace HTool.Format.Pro.Job;

/// <summary>
///     체결 스텝 본문. 툴, 프리셋, 나사 위치, 가상 프리셋, 재체결, 인코더 데이터를 포함합니다.
///     fastening step body. contains tool, preset, screw positions, virtual preset, re-tight, and encoder data.
/// </summary>
/// <remarks>
///     리틀엔디안 바이너리 형식. Rev.1에서 인코더 데이터가 추가됩니다.
///     little-endian binary format. encoder data is added in Rev.1.
/// </remarks>
public sealed class FastenBody : IStepBody {
	/// <summary>
	///     최대 나사 수
	///     maximum screw count
	/// </summary>
	public const int MaxScrewCount = 99;

	/// <summary>
	///     툴 이름 필드 크기 (바이트)
	///     tool name field size (bytes)
	/// </summary>
	private const int ToolNameFieldSize = 36;

	/// <summary>
	///     이미지 경로 필드 크기 (바이트)
	///     image path field size (bytes)
	/// </summary>
	private const int ImagePathFieldSize = 256;

	/// <summary>
	///     가상 프리셋 데이터 크기 (바이트)
	///     virtual preset data size (bytes)
	/// </summary>
	private const int VirtualDataSize = 100;

	/// <summary>
	///     기본 생성자. 나사 및 인코더 배열을 초기화합니다.
	///     default constructor. initializes screw and encoder arrays.
	/// </summary>
	public FastenBody() {
        // 나사 배열 초기화
        // initialize screw array
        for (var i = 0; i < MaxScrewCount; i++)
            // 새 나사 인스턴스 생성
            // create new screw instance
            Screws[i] = new Screw();
        // 인코더 배열 초기화
        // initialize encoder array
        for (var i = 0; i < MaxScrewCount; i++)
            // 새 인코더 인스턴스 생성
            // create new encoder instance
            Encoders[i] = new ScrewEncoder();
    }

	/// <summary>
	///     툴 이름 (36바이트)
	///     tool name (36 bytes)
	/// </summary>
	public string ToolName { get; set; } = string.Empty;

	/// <summary>
	///     프리셋 번호
	///     preset number
	/// </summary>
	public int Preset { get; set; }

	/// <summary>
	///     나사 수
	///     screw count
	/// </summary>
	public int CountOfScrew { get; set; }

	/// <summary>
	///     이미지 사용 여부
	///     image enabled
	/// </summary>
	public bool IsImage { get; set; }

	/// <summary>
	///     이미지 파일 경로 (256바이트)
	///     image file path (256 bytes)
	/// </summary>
	public string ImagePath { get; set; } = string.Empty;

	/// <summary>
	///     나사 위치 데이터 (99개)
	///     screw position data (99 entries)
	/// </summary>
	public Screw[] Screws { get; } = new Screw[MaxScrewCount];

	/// <summary>
	///     가상 프리셋 데이터
	///     virtual preset data
	/// </summary>
	public VirtualPreset Virtual { get; } = new();

	/// <summary>
	///     재체결 사용 여부
	///     re-tight enabled
	/// </summary>
	public bool IsReTight { get; set; }

	/// <summary>
	///     최대 재체결 횟수
	///     maximum re-tight count
	/// </summary>
	public int MaxReTight { get; set; }

	/// <summary>
	///     재체결 프리셋 번호
	///     re-tight preset number
	/// </summary>
	public int ReTightPreset { get; set; }

	/// <summary>
	///     재체결 가상 프리셋 데이터
	///     re-tight virtual preset data
	/// </summary>
	public VirtualPreset ReTightVirtual { get; } = new();

	/// <summary>
	///     소켓 트레이 번호
	///     socket tray number
	/// </summary>
	public int SocketNumber { get; set; }

	/// <summary>
	///     스텝 유형 (체결)
	///     step type (fastening)
	/// </summary>
	public JobStep StepType => JobStep.Fastening;

	/// <summary>
	///     본문을 바이트 배열로 직렬화합니다 (리틀엔디안).
	///     serializes body to a byte array (little-endian).
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] ToBytes(int revision = 0) {
        // 스트림 생성
        // create stream
        using var stream = new MemoryStream();
        // 바이너리 라이터 생성
        // create binary writer
        using var bin = new BinaryWriter(stream);

        // 툴 이름 바이트 변환
        // convert tool name to bytes
        var toolNameBytes = Encoding.ASCII.GetBytes(ToolName);
        // 툴 이름 데이터 쓰기
        // write tool name data
        bin.Write(toolNameBytes);
        // 툴 이름 패딩 쓰기
        // write tool name padding
        bin.Write(new byte[ToolNameFieldSize - toolNameBytes.Length]);
        // 프리셋 번호 쓰기
        // write preset number
        bin.Write(Preset);
        // 나사 수 쓰기
        // write screw count
        bin.Write(CountOfScrew);
        // 이미지 사용 여부 쓰기 (bool → int32)
        // write image enabled (bool -> int32)
        bin.Write(Convert.ToInt32(IsImage));
        // 이미지 경로 바이트 변환
        // convert image path to bytes
        var imagePathBytes = Encoding.ASCII.GetBytes(ImagePath);
        // 이미지 경로 데이터 쓰기
        // write image path data
        bin.Write(imagePathBytes);
        // 이미지 경로 패딩 쓰기
        // write image path padding
        bin.Write(new byte[ImagePathFieldSize - imagePathBytes.Length]);

        // 나사 99개 쓰기
        // write 99 screws
        foreach (var screw in Screws) {
            // 활성화 상태 쓰기 (bool → int32)
            // write enable status (bool -> int32)
            bin.Write(Convert.ToInt32(screw.Enable));
            // X 좌표 쓰기
            // write X position
            bin.Write(screw.X);
            // Y 좌표 쓰기
            // write Y position
            bin.Write(screw.Y);
            // 반경 쓰기
            // write radius
            bin.Write(screw.Radius);
            // 두께 쓰기
            // write thickness
            bin.Write(screw.Thickness);
            // 기본 색상 R 쓰기
            // write default color R
            bin.Write(screw.DefaultColor.R);
            // 기본 색상 G 쓰기
            // write default color G
            bin.Write(screw.DefaultColor.G);
            // 기본 색상 B 쓰기
            // write default color B
            bin.Write(screw.DefaultColor.B);
            // OK 색상 R 쓰기
            // write OK color R
            bin.Write(screw.OkColor.R);
            // OK 색상 G 쓰기
            // write OK color G
            bin.Write(screw.OkColor.G);
            // OK 색상 B 쓰기
            // write OK color B
            bin.Write(screw.OkColor.B);
            // NG 색상 R 쓰기
            // write NG color R
            bin.Write(screw.NgColor.R);
            // NG 색상 G 쓰기
            // write NG color G
            bin.Write(screw.NgColor.G);
            // NG 색상 B 쓰기
            // write NG color B
            bin.Write(screw.NgColor.B);
            // 예비 바이트 쓰기
            // write spare bytes
            bin.Write(screw.Spare);
        }

        // 가상 프리셋 활성화 쓰기
        // write virtual preset enable
        bin.Write(Convert.ToInt32(Virtual.Enable));
        // 가상 프리셋 체결 데이터 쓰기
        // write virtual preset fasten data
        bin.Write(Virtual.Fasten);
        // 가상 프리셋 어드밴스 데이터 쓰기
        // write virtual preset advance data
        bin.Write(Virtual.Advance);

        // 재체결 상태 쓰기
        // write re-tight status
        bin.Write(Convert.ToInt32(IsReTight));
        // 최대 재체결 횟수 쓰기
        // write maximum re-tight count
        bin.Write(MaxReTight);
        // 재체결 프리셋 번호 쓰기
        // write re-tight preset number
        bin.Write(ReTightPreset);

        // 재체결 가상 프리셋 활성화 쓰기
        // write re-tight virtual preset enable
        bin.Write(Convert.ToInt32(ReTightVirtual.Enable));
        // 재체결 가상 프리셋 체결 데이터 쓰기
        // write re-tight virtual preset fasten data
        bin.Write(ReTightVirtual.Fasten);
        // 재체결 가상 프리셋 어드밴스 데이터 쓰기
        // write re-tight virtual preset advance data
        bin.Write(ReTightVirtual.Advance);

        // 소켓 트레이 번호 쓰기
        // write socket tray number
        bin.Write(SocketNumber);

        // 리비전 1 이상 데이터 직렬화
        // serialize revision 1+ data
        if (revision >= 1) {
            // 인코더 사용 여부 쓰기 (bool → int32)
            // write encoder enabled (bool -> int32)
            bin.Write(Convert.ToInt32(EnableEncoder));
            // 비순차 사용 여부 쓰기 (bool → int32)
            // write non-sequential enabled (bool -> int32)
            bin.Write(Convert.ToInt32(EnableNonSeq));

            // 인코더 99개 쓰기
            // write 99 encoders
            foreach (var enc in Encoders) {
                // 저장 위치 4개 쓰기
                // write 4 save positions
                for (var j = 0; j < 4; j++)
                    // 위치 값 쓰기
                    // write position value
                    bin.Write(enc.SavePos[j]);
                // 존 허용 오차 4개 쓰기
                // write 4 zone tolerances
                for (var j = 0; j < 4; j++)
                    // 허용 오차 값 쓰기
                    // write tolerance value
                    bin.Write(enc.ZoneTol[j]);
                // OK 허용 오차 4개 쓰기
                // write 4 OK tolerances
                for (var j = 0; j < 4; j++)
                    // 허용 오차 값 쓰기
                    // write tolerance value
                    bin.Write(enc.OkTol[j]);
                // 픽업 활성화 2개 쓰기 (bool → int32)
                // write 2 pick-up enables (bool -> int32)
                for (var j = 0; j < 2; j++)
                    // 픽업 상태 쓰기
                    // write pick-up status
                    bin.Write(Convert.ToInt32(enc.EnabledPickUp[j]));
            }
        }

        // 직렬화된 배열 반환
        // return serialized array
        return stream.ToArray();
    }

	/// <summary>
	///     원시 스팬에서 체결 본문을 파싱합니다 (리틀엔디안).
	///     parses fastening body from raw span (little-endian).
	/// </summary>
	/// <param name="data">헤더 이후의 데이터 스팬 / data span after header</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>파싱된 체결 본문 / parsed fastening body</returns>
	public static FastenBody Parse(ReadOnlySpan<byte> data, int revision = 0) {
        // 본문 인스턴스 생성
        // create body instance
        var body = new FastenBody();
        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 툴 이름 읽기 (36바이트 ASCII)
        // read tool name (36 bytes ASCII)
        body.ToolName = Encoding.ASCII.GetString(data.Slice(pos, ToolNameFieldSize)).TrimEnd('\0');
        // 툴 이름 이후로 위치 이동
        // advance position past tool name
        pos += ToolNameFieldSize;
        // 프리셋 번호 읽기
        // read preset number
        body.Preset = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 나사 수 읽기
        // read screw count
        body.CountOfScrew = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 이미지 사용 여부 읽기 (int32 → bool)
        // read image enabled (int32 -> bool)
        body.IsImage = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 이미지 경로 읽기 (256바이트 ASCII)
        // read image path (256 bytes ASCII)
        body.ImagePath = Encoding.ASCII.GetString(data.Slice(pos, ImagePathFieldSize)).TrimEnd('\0');
        // 이미지 경로 이후로 위치 이동
        // advance position past image path
        pos += ImagePathFieldSize;

        // 나사 99개 읽기
        // read 99 screws
        for (var i = 0; i < MaxScrewCount; i++) {
            // 나사 참조 가져오기
            // get screw reference
            var screw = body.Screws[i];
            // 활성화 상태 읽기 (int32 → bool)
            // read enable status (int32 -> bool)
            screw.Enable = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // X 좌표 읽기
            // read X position
            screw.X = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // Y 좌표 읽기
            // read Y position
            screw.Y = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // 반경 읽기
            // read radius
            screw.Radius = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // 두께 읽기
            // read thickness
            screw.Thickness = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // 기본 색상 읽기 (R, G, B)
            // read default color (R, G, B)
            screw.DefaultColor = new ScrewColor(data[pos], data[pos + 1], data[pos + 2]);
            // 위치 3바이트 이동
            // advance position by 3 bytes
            pos += 3;
            // OK 색상 읽기 (R, G, B)
            // read OK color (R, G, B)
            screw.OkColor = new ScrewColor(data[pos], data[pos + 1], data[pos + 2]);
            // 위치 3바이트 이동
            // advance position by 3 bytes
            pos += 3;
            // NG 색상 읽기 (R, G, B)
            // read NG color (R, G, B)
            screw.NgColor = new ScrewColor(data[pos], data[pos + 1], data[pos + 2]);
            // 위치 3바이트 이동
            // advance position by 3 bytes
            pos += 3;
            // 예비 바이트 읽기 (3바이트)
            // read spare bytes (3 bytes)
            data.Slice(pos, 3).CopyTo(screw.Spare);
            // 위치 3바이트 이동
            // advance position by 3 bytes
            pos += 3;
        }

        // 가상 프리셋 읽기
        // read virtual preset
        body.Virtual.Enable = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 가상 프리셋 체결 데이터 읽기
        // read virtual preset fasten data
        data.Slice(pos, VirtualDataSize).CopyTo(body.Virtual.Fasten);
        // 가상 체결 데이터 이후로 위치 이동
        // advance position past virtual fasten data
        pos += VirtualDataSize;
        // 가상 프리셋 어드밴스 데이터 읽기
        // read virtual preset advance data
        data.Slice(pos, VirtualDataSize).CopyTo(body.Virtual.Advance);
        // 가상 어드밴스 데이터 이후로 위치 이동
        // advance position past virtual advance data
        pos += VirtualDataSize;

        // 재체결 상태 읽기 (int32 → bool)
        // read re-tight status (int32 -> bool)
        body.IsReTight = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 최대 재체결 횟수 읽기
        // read maximum re-tight count
        body.MaxReTight = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 재체결 프리셋 번호 읽기
        // read re-tight preset number
        body.ReTightPreset = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;

        // 재체결 가상 프리셋 읽기
        // read re-tight virtual preset
        body.ReTightVirtual.Enable = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;
        // 재체결 가상 프리셋 체결 데이터 읽기
        // read re-tight virtual preset fasten data
        data.Slice(pos, VirtualDataSize).CopyTo(body.ReTightVirtual.Fasten);
        // 재체결 체결 데이터 이후로 위치 이동
        // advance position past re-tight fasten data
        pos += VirtualDataSize;
        // 재체결 가상 프리셋 어드밴스 데이터 읽기
        // read re-tight virtual preset advance data
        data.Slice(pos, VirtualDataSize).CopyTo(body.ReTightVirtual.Advance);
        // 재체결 어드밴스 데이터 이후로 위치 이동
        // advance position past re-tight advance data
        pos += VirtualDataSize;

        // 소켓 트레이 번호 읽기
        // read socket tray number
        body.SocketNumber = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
        // 위치 4바이트 이동
        // advance position by 4 bytes
        pos += 4;

        // 리비전 1 이상 데이터 파싱
        // parse revision 1+ data
        if (revision >= 1) {
            // 인코더 사용 여부 읽기 (int32 → bool)
            // read encoder enabled (int32 -> bool)
            body.EnableEncoder = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;
            // 비순차 사용 여부 읽기 (int32 → bool)
            // read non-sequential enabled (int32 -> bool)
            body.EnableNonSeq = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
            // 위치 4바이트 이동
            // advance position by 4 bytes
            pos += 4;

            // 인코더 99개 읽기
            // read 99 encoders
            for (var i = 0; i < MaxScrewCount; i++) {
                // 인코더 참조 가져오기
                // get encoder reference
                var enc = body.Encoders[i];
                // 저장 위치 4개 읽기
                // read 4 save positions
                for (var j = 0; j < 4; j++) {
                    // 위치 값 읽기
                    // read position value
                    enc.SavePos[j] = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
                    // 위치 4바이트 이동
                    // advance position by 4 bytes
                    pos += 4;
                }

                // 존 허용 오차 4개 읽기
                // read 4 zone tolerances
                for (var j = 0; j < 4; j++) {
                    // 허용 오차 값 읽기
                    // read tolerance value
                    enc.ZoneTol[j] = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
                    // 위치 4바이트 이동
                    // advance position by 4 bytes
                    pos += 4;
                }

                // OK 허용 오차 4개 읽기
                // read 4 OK tolerances
                for (var j = 0; j < 4; j++) {
                    // 허용 오차 값 읽기
                    // read tolerance value
                    enc.OkTol[j] = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]);
                    // 위치 4바이트 이동
                    // advance position by 4 bytes
                    pos += 4;
                }

                // 픽업 활성화 2개 읽기 (int32 → bool)
                // read 2 pick-up enables (int32 -> bool)
                for (var j = 0; j < 2; j++) {
                    // 픽업 상태 읽기
                    // read pick-up status
                    enc.EnabledPickUp[j] = BinaryPrimitives.ReadInt32LittleEndian(data[pos..]) != 0;
                    // 위치 4바이트 이동
                    // advance position by 4 bytes
                    pos += 4;
                }
            }
        }

        // 파싱된 본문 반환
        // return parsed body
        return body;
    }

	/// <summary>
	///     나사 위치 및 색상 데이터.
	///     screw position and color data.
	/// </summary>
	public sealed class Screw {
		/// <summary>
		///     활성화 상태
		///     enable status
		/// </summary>
		public bool Enable { get; set; }

		/// <summary>
		///     X 좌표
		///     X position
		/// </summary>
		public uint X { get; set; }

		/// <summary>
		///     Y 좌표
		///     Y position
		/// </summary>
		public uint Y { get; set; }

		/// <summary>
		///     반경
		///     radius
		/// </summary>
		public uint Radius { get; set; }

		/// <summary>
		///     두께
		///     thickness
		/// </summary>
		public uint Thickness { get; set; }

		/// <summary>
		///     기본 색상 (RGB)
		///     default color (RGB)
		/// </summary>
		public ScrewColor DefaultColor { get; set; }

		/// <summary>
		///     OK 색상 (RGB)
		///     OK color (RGB)
		/// </summary>
		public ScrewColor OkColor { get; set; }

		/// <summary>
		///     NG 색상 (RGB)
		///     NG color (RGB)
		/// </summary>
		public ScrewColor NgColor { get; set; }

		/// <summary>
		///     예비 바이트 (3바이트)
		///     spare bytes (3 bytes)
		/// </summary>
		public byte[] Spare { get; } = new byte[3];
    }

	/// <summary>
	///     나사 색상 (R, G, B).
	///     screw color (R, G, B).
	/// </summary>
	/// <param name="R">빨강 / red</param>
	/// <param name="G">초록 / green</param>
	/// <param name="B">파랑 / blue</param>
	public record struct ScrewColor(byte R, byte G, byte B);

	/// <summary>
	///     가상 프리셋 데이터 (활성화 + 체결 + 어드밴스).
	///     virtual preset data (enable + fasten + advance).
	/// </summary>
	public sealed class VirtualPreset {
		/// <summary>
		///     활성화 상태
		///     enable status
		/// </summary>
		public bool Enable { get; set; }

		/// <summary>
		///     체결 데이터 (100바이트)
		///     fasten data (100 bytes)
		/// </summary>
		public byte[] Fasten { get; } = new byte[100];

		/// <summary>
		///     어드밴스 데이터 (100바이트)
		///     advance data (100 bytes)
		/// </summary>
		public byte[] Advance { get; } = new byte[100];
    }

	/// <summary>
	///     나사 인코더 데이터 (Rev.1).
	///     screw encoder data (Rev.1).
	/// </summary>
	public sealed class ScrewEncoder {
		/// <summary>
		///     기본 생성자. 배열을 초기화합니다.
		///     default constructor. initializes arrays.
		/// </summary>
		public ScrewEncoder() {
            // 저장 위치 초기화
            // initialize save positions
            SavePos = [0, 0, 0, 0];
            // 존 허용 오차 초기화
            // initialize zone tolerances
            ZoneTol = [0, 0, 0, 0];
            // OK 허용 오차 초기화
            // initialize OK tolerances
            OkTol = [0, 0, 0, 0];
            // 픽업 활성화 초기화
            // initialize pick-up enables
            EnabledPickUp = [false, false];
        }

		/// <summary>
		///     저장된 인코더 위치 (4개)
		///     saved encoder positions (4 entries)
		/// </summary>
		public int[] SavePos { get; }

		/// <summary>
		///     존 허용 오차 (4개)
		///     zone tolerances (4 entries)
		/// </summary>
		public int[] ZoneTol { get; }

		/// <summary>
		///     OK 허용 오차 (4개)
		///     OK tolerances (4 entries)
		/// </summary>
		public int[] OkTol { get; }

		/// <summary>
		///     픽업 활성화 (2개)
		///     pick-up enables (2 entries)
		/// </summary>
		public bool[] EnabledPickUp { get; }
    }

    #region Rev.1

    /// <summary>
    ///     인코더 사용 여부 (Rev.1)
    ///     encoder enabled (Rev.1)
    /// </summary>
    public bool EnableEncoder { get; set; }

    /// <summary>
    ///     비순차 체결 사용 여부 (Rev.1)
    ///     non-sequential fastening enabled (Rev.1)
    /// </summary>
    public bool EnableNonSeq { get; set; }

    /// <summary>
    ///     인코더 데이터 (99개, Rev.1)
    ///     encoder data (99 entries, Rev.1)
    /// </summary>
    public ScrewEncoder[] Encoders { get; } = new ScrewEncoder[MaxScrewCount];

    #endregion
}