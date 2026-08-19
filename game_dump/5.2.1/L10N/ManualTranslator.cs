using Durango.System;

namespace L10N;

public static class ManualTranslator
{
	public static string Recommended => LocalizeSystem.Locale switch
	{
		"ko_KR" => "추천", 
		"ru_RU" => "Рекомендуется", 
		"es_MX" => "Recomendado", 
		"th_TH" => "แนะนำ", 
		"ja_JP" => "おすすめ", 
		"de_DE" => "Empfohlen", 
		"fr_FR" => "Recommandé", 
		"zh_TW" => "推薦", 
		"pt_BR" => "Recomendado", 
		"id_ID" => "Direkomendasikan", 
		_ => "Recommended", 
	};

	public static string LoadingUserInfo => LocalizeSystem.Locale switch
	{
		"ko_KR" => "사용자 정보를 가져오는 중입니다.", 
		"ru_RU" => "Загрузка информации об игроке.", 
		"es_MX" => "Cargando información del jugador.", 
		"th_TH" => "กำล\u0e31งโหลดข\u0e49อม\u0e39ลผ\u0e39\u0e49เล\u0e48น", 
		"ja_JP" => "プレイヤー情報を読み込んでいます。", 
		"de_DE" => "Spielerdaten werden geladen.", 
		"fr_FR" => "Chargement des informations du joueur.", 
		"zh_TW" => "使用者資訊載入中。", 
		"pt_BR" => "Verificando dados do jogo.", 
		"id_ID" => "Memuat informasi pemain.", 
		_ => "Loading player information.", 
	};

	public static string CheckingGameData => LocalizeSystem.Locale switch
	{
		"ko_KR" => "게임 데이터를 확인 중입니다.", 
		"ru_RU" => "Проверка данных игры.", 
		"es_MX" => "Comprobando datos del juego.", 
		"th_TH" => "กำล\u0e31งตรวจสอบข\u0e49อม\u0e39ลเกม", 
		"ja_JP" => "ゲームデータを確認しています。", 
		"de_DE" => "Spieldaten werden überprüft.", 
		"fr_FR" => "Vérification des données de jeu.", 
		"zh_TW" => "遊戲資料確認中。", 
		"pt_BR" => "Carregando informações do jogador.", 
		"id_ID" => "Memeriksa data game.", 
		_ => "Checking game data.", 
	};

	public static string NoCharacter => LocalizeSystem.Locale switch
	{
		"ko_KR" => "캐릭터 없음", 
		"ru_RU" => "Нет персонажа", 
		"es_MX" => "No hay personaje", 
		"th_TH" => "ไม\u0e48ม\u0e35ต\u0e31วละคร", 
		"ja_JP" => "キャラクターなし", 
		"de_DE" => "Kein Charakter", 
		"fr_FR" => "Aucun personnage", 
		"zh_TW" => "無角色", 
		"pt_BR" => "Não há personagem", 
		"id_ID" => "Tidak Ada Karakter", 
		_ => "No Character", 
	};

	public static string Loading => LocalizeSystem.Locale switch
	{
		"ko_KR" => "확인 중", 
		"ru_RU" => "Проверка...", 
		"es_MX" => "Revisando...", 
		"th_TH" => "กำล\u0e31งตรวจสอบ...", 
		"ja_JP" => "確認中", 
		"de_DE" => "Wird geprüft …", 
		"fr_FR" => "Vérification...", 
		"zh_TW" => "確認中", 
		"pt_BR" => "Verificando...", 
		"id_ID" => "Memeriksa...", 
		_ => "Checking...", 
	};

	public static string Maintainance => LocalizeSystem.Locale switch
	{
		"ko_KR" => "점검 중입니다.", 
		"ru_RU" => "Ведутся технические работы.", 
		"es_MX" => "Actualmente bajo mantenimiento.", 
		"th_TH" => "กำล\u0e31งบำร\u0e38งร\u0e31กษา", 
		"ja_JP" => "メンテナンス中です。", 
		"de_DE" => "Laufende Wartungsarbeiten.", 
		"fr_FR" => "Le jeu est en cours de maintenance.", 
		"zh_TW" => "維護中。", 
		"pt_BR" => "Manutenção em andamento.", 
		"id_ID" => "Sedang dalam perbaikan.", 
		_ => "Currently under maintenance.", 
	};

	public static string PermissionDenied => LocalizeSystem.Locale switch
	{
		"ko_KR" => "권한이 거부되었습니다.", 
		"ru_RU" => "Доступ запрещён.", 
		"es_MX" => "Acceso denegado.", 
		"th_TH" => "การเข\u0e49าถ\u0e36งถ\u0e39กปฏ\u0e34เสธ", 
		"ja_JP" => "アクセスが拒否されました。", 
		"de_DE" => "Zugriff verweigert.", 
		"fr_FR" => "Accès refusé.", 
		"zh_TW" => "權限存取被拒。", 
		"pt_BR" => "Acesso negado.", 
		"id_ID" => "Akses ditolak.", 
		_ => "Access denied.", 
	};

	public static string LoginFailed => LocalizeSystem.Locale switch
	{
		"ko_KR" => "로그인에 실패했습니다.", 
		"ru_RU" => "Не удалось войти.", 
		"es_MX" => "Falla al iniciar sesión.", 
		"th_TH" => "การล\u0e47อกอ\u0e34นล\u0e49มเหลว", 
		"ja_JP" => "ログインに失敗しました。", 
		"de_DE" => "Login fehlgeschlagen.", 
		"fr_FR" => "Échec de la connexion.", 
		"zh_TW" => "登入失敗。", 
		"pt_BR" => "Falha no login.", 
		"id_ID" => "Login gagal.", 
		_ => "Login failed.", 
	};

	public static string Banned => LocalizeSystem.Locale switch
	{
		"ko_KR" => "운영 정책을 위반하여 게임 이용 제한중입니다.\n관련 문의는 넥슨닷컴>고객센터>야생의 땅: 듀랑고 1:1 문의를 이용해주세요.", 
		"ru_RU" => "На вашу учетную запись были наложены ограничения из-за нарушения правил использования.\nЕсли у вас возникли вопросы, перейдите по адресу www.m.nexon.com » Помощь и поддержка » Поддержка по Durango: Wild Lands 1:1", 
		"es_MX" => "Se aplicaron restricciones a tu cuenta debido a un incumplimiento con nuestras políticas de operación.\nSi tienes alguna pregunta, dirígete a www.m.nexon.com » Ayuda y soporte » Durango: Wild Lands, soporte individual.", 
		"th_TH" => "บ\u0e31ญช\u0e35ของค\u0e38ณถ\u0e39กระง\u0e31บการใช\u0e49งานเน\u0e37\u0e48องจากฝ\u0e48าฝ\u0e37นนโยบายการดำเน\u0e34นงานของเรา\nหากม\u0e35ข\u0e49อสงส\u0e31ย กร\u0e38ณาไปท\u0e35\u0e48 www.m.nexon.com » ความช\u0e48วยเหล\u0e37อและการสน\u0e31บสน\u0e38น » Durango: Wild Lands 1:1 สน\u0e31บสน\u0e38น", 
		"ja_JP" => "運営ポリシーに違反したため、ゲームの利用が制限されています。\n詳しくは、nexon.com>サポート>DURANGO：Wild Landsのお問い合わせをご利用ください。", 
		"de_DE" => "Der Zugriff auf dein Konto wurde aufgrund eines Verstoßes gegen unsere Richtlinien eingeschränkt.\nBei Fragen besuche m.nexon.com und gehe auf Help and Support » Durango: Wild Lands Persönlicher Support.", 
		"fr_FR" => "Votre compte a été limité en raison d'une violation des règles du jeu.\nPour toute question, rendez - vous sur www.m.nexon.com » Help and Support » Support direct Durango: Wild Lands", 
		"zh_TW" => "違反營運政策，限制使用遊戲中。\n相關疑問請利用nexon.com>客服中心>Durango: 野生之地進行1:1詢問。", 
		"pt_BR" => "A sua conta foi restringida devido à uma violação de nossas políticas operacionais.\nCaso tenha alguma pergunta, acesse www.m.nexon.com » Ajuda e Suporte » Durango: Wild Lands 1:1 suporte.", 
		"id_ID" => "Akun Anda telah dibatasi karena pelanggaran terhadap kebijakan operasi kami.\nJika Anda memiliki pertanyaan, buka www.m.nexon.com » Help and Support » Durango: Wild Lands 1:1.", 
		_ => "Your account has been restricted due to a violation of our operating policies.\nIf you have any questions, please go to www.m.nexon.com » Help and Support » Durango: Wild Lands 1:1 support.", 
	};

	public static string DataLoadError => LocalizeSystem.Locale switch
	{
		"ko_KR" => "데이터를 불러오는 데 실패하였습니다.", 
		"ru_RU" => "Не удалось загрузить данные.", 
		"es_MX" => "Falla al recuperar datos.", 
		"th_TH" => "การเร\u0e35ยกข\u0e49อม\u0e39ลล\u0e49มเหลว", 
		"ja_JP" => "データの読み込みに失敗しました。", 
		"de_DE" => "Daten konnten nicht abgerufen werden.", 
		"fr_FR" => "Récupération des données impossible.", 
		"zh_TW" => "資料載入失敗。", 
		"pt_BR" => "Falha ao recuperar dados.", 
		"id_ID" => "Gagal memanggil data.", 
		_ => "Failed to retrieve data.", 
	};

	public static string InvalidArguments => LocalizeSystem.Locale switch
	{
		"ko_KR" => "비정상적인 경로로 실행되었습니다.", 
		"zh_TW" => "執行路徑不正常。", 
		"th_TH" => "ตรวจพบเส\u0e49นทางการเป\u0e34ดใช\u0e49ผ\u0e34ดปกต\u0e34", 
		"es_MX" => "Ruta de lanzamiento irregular detectada.", 
		"ru_RU" => "Обнаружен нестандартный путь запуска.", 
		"de_DE" => "Ungültiger Startpfad festgestellt.", 
		"fr_FR" => "Chemin de lancement inhabituel détecté.", 
		"pt_BR" => "Caminho irregular de inicialização detectado", 
		"id_ID" => "Jalur peluncuran tak biasa terdeteksi.", 
		_ => "Irregular launch path detected.", 
	};

	public static string InitializeFailed => LocalizeSystem.Locale switch
	{
		"ko_KR" => "초기화에 실패했습니다.", 
		"zh_TW" => "初始化失敗。", 
		"th_TH" => "การร\u0e35เซ\u0e47ตล\u0e49มเหลว", 
		"es_MX" => "Falla al reiniciar.", 
		"ru_RU" => "Неудачный сброс.", 
		"de_DE" => "Neustart fehlgeschlagen.", 
		"fr_FR" => "Échec de la réinitialisation.", 
		"pt_BR" => "Redefinição falhou", 
		"id_ID" => "Reset gagal.", 
		_ => "Initialization has failed.", 
	};

	public static string TouchTheScreen => LocalizeSystem.Locale switch
	{
		"ko_KR" => "화면을 터치해 주세요.", 
		"ja_JP" => "画面をタップしてください。", 
		"zh_TW" => "請點擊畫面。", 
		"th_TH" => "แตะหน\u0e49าจอ", 
		"es_MX" => "Toca la pantalla.", 
		"ru_RU" => "Коснитесь экрана.", 
		"de_DE" => "Tippe auf den Bildschirm.", 
		"fr_FR" => "Touchez l'écran.", 
		"pt_BR" => "Toque na tela.", 
		"id_ID" => "Ketuk layar.", 
		_ => "Tap the screen.", 
	};

	public static string ServerSelection => LocalizeSystem.Locale switch
	{
		"ko_KR" => "서버 선택", 
		"ja_JP" => "サーバー選択", 
		"zh_TW" => "選擇伺服器", 
		"th_TH" => "เล\u0e37อกเซ\u0e34ร\u0e4cฟเวอร\u0e4c", 
		"es_MX" => "Seleccionar servidor", 
		"ru_RU" => "Выбрать сервер", 
		"de_DE" => "Server auswählen", 
		"fr_FR" => "Sélectionnez le serveur.", 
		"pt_BR" => "Selecionar Servidor", 
		"id_ID" => "Pilih Server", 
		_ => "Select Server", 
	};

	public static string Confirm => LocalizeSystem.Locale switch
	{
		"ko_KR" => "확인", 
		"ja_JP" => "OK", 
		"zh_TW" => "確定", 
		"th_TH" => "ตกลง", 
		"es_MX" => "Aceptar", 
		"ru_RU" => "OK", 
		"de_DE" => "OK", 
		"fr_FR" => "OK", 
		"pt_BR" => "OK", 
		"id_ID" => "OKE", 
		_ => "OK", 
	};

	public static string Cancel => LocalizeSystem.Locale switch
	{
		"ko_KR" => "취소", 
		"ja_JP" => "キャンセル", 
		"zh_TW" => "取消", 
		"th_TH" => "ยกเล\u0e34ก", 
		"es_MX" => "Cancelar", 
		"ru_RU" => "Отмена", 
		"de_DE" => "Abbrechen", 
		"fr_FR" => "Annuler", 
		"pt_BR" => "Cancelar", 
		"id_ID" => "Batal", 
		_ => "Cancel", 
	};

	public static string TapToClose => LocalizeSystem.Locale switch
	{
		"ko_KR" => "화면을 터치하면 게임이 종료됩니다.", 
		"ja_JP" => "画面をタップするとゲームが終了します。", 
		"zh_TW" => "點擊畫面後，遊戲即結束。", 
		"th_TH" => " โปรดแตะหน\u0e49าจอเพ\u0e37\u0e48อป\u0e34ดเกม", 
		"es_MX" => " Toca la pantalla para cerrar el juego.", 
		"ru_RU" => " Коснитесь экрана, чтобы закрыть игру.", 
		"de_DE" => " Zum Schließen des Spiels auf den Bildschirm tippen.", 
		"fr_FR" => " Touchez l'écran pour quitter.", 
		"pt_BR" => " Toque a tela para fechar o jogo.", 
		"id_ID" => "Ketuk layar untuk menutup game.", 
		_ => "Tap the screen to close the game.", 
	};

	public static string TapToRetry => LocalizeSystem.Locale switch
	{
		"ko_KR" => "화면을 터치 후 다시 시도해 주세요.", 
		"ja_JP" => "画面をタップしてもう一度お試しください。", 
		"zh_TW" => "請點擊畫面後重試。", 
		"th_TH" => " โปรดแตะหน\u0e49าจอเพ\u0e37\u0e48อลองใหม\u0e48อ\u0e35กคร\u0e31\u0e49ง", 
		"es_MX" => " Toca la pantalla para intentarlo de nuevo.", 
		"ru_RU" => " Коснитесь экрана, чтобы попробовать снова.", 
		"de_DE" => " Auf den Bildschirm tippen, um es erneut zu versuchen.", 
		"fr_FR" => " Touchez l'écran pour réessayer.", 
		"pt_BR" => " Toque a tela para tentar novamente.", 
		"id_ID" => "Ketuk layar untuk mengulang.", 
		_ => "Tap the screen to retry.", 
	};

	public static string Close => LocalizeSystem.Locale switch
	{
		"ko_KR" => "종료", 
		"ja_JP" => "Close", 
		"zh_TW" => "結束", 
		"th_TH" => "ป\u0e34ด", 
		"es_MX" => "Cerrar", 
		"ru_RU" => "Закрыть", 
		"de_DE" => "Schließen", 
		"fr_FR" => "Fermer", 
		"pt_BR" => "Fechar", 
		"id_ID" => "Tutup", 
		_ => "Close", 
	};

	public static string Retry => LocalizeSystem.Locale switch
	{
		"ko_KR" => "다시 시도", 
		"ja_JP" => "Retry", 
		"zh_TW" => "重試", 
		"th_TH" => "ลองใหม\u0e48", 
		"es_MX" => "Reintentar", 
		"ru_RU" => "Еще раз", 
		"de_DE" => "Erneut versuchen", 
		"fr_FR" => "Réessayer", 
		"pt_BR" => "Tentar Novamente", 
		"id_ID" => "Ulang", 
		_ => "Retry", 
	};

	public static string Start => LocalizeSystem.Locale switch
	{
		"ko_KR" => "게임 시작", 
		"ja_JP" => "Start", 
		"zh_TW" => "開始遊戲", 
		"th_TH" => "เร\u0e34\u0e48ม", 
		"es_MX" => "Iniciar", 
		"ru_RU" => "Начать", 
		"de_DE" => "Start", 
		"fr_FR" => "Commencer", 
		"pt_BR" => "Iniciar", 
		"id_ID" => "Mulai", 
		_ => "Start", 
	};

	public static string WantToQuit => LocalizeSystem.Locale switch
	{
		"ko_KR" => "종료하시겠습니까?", 
		"ja_JP" => "終了しますか？", 
		"zh_TW" => "要結束嗎？", 
		"th_TH" => "ค\u0e38ณต\u0e49องการออกจร\u0e34งๆ ใช\u0e48หร\u0e37อไม\u0e48", 
		"es_MX" => "¿Deseas abandonar?", 
		"ru_RU" => "Вы точно хотите выйти?", 
		"de_DE" => "Wirklich beenden?", 
		"fr_FR" => "Voulez-vous vraiment quitter\u00a0?", 
		"pt_BR" => "Tem certeza de que deseja sair?", 
		"id_ID" => "Apa kamu benar-benar ingin berhenti?", 
		_ => "Do you really want to quit?", 
	};

	public static string ExitGame => LocalizeSystem.Locale switch
	{
		"ko_KR" => "게임 나가기", 
		"zh_TW" => "離開遊戲", 
		"th_TH" => "ออกจากเกม", 
		"es_MX" => "Salir del juego", 
		"ru_RU" => "Выйти из игры", 
		"de_DE" => "Spiel beenden", 
		"fr_FR" => "Quitter le jeu", 
		"pt_BR" => "Sair do Jogo", 
		"id_ID" => "Keluar dari Game", 
		_ => "Exit Game", 
	};

	public static string SelectCharacter => LocalizeSystem.Locale switch
	{
		"ko_KR" => "캐릭터 선택", 
		"ja_JP" => "キャラクター選択", 
		"zh_TW" => "選擇角色", 
		"th_TH" => "เล\u0e37อกต\u0e31วละคร", 
		"es_MX" => "Seleccionar personaje", 
		"ru_RU" => "Выберите персонажа", 
		"de_DE" => "Charakter auswählen", 
		"fr_FR" => "Sélectionner le personnage", 
		"pt_BR" => "Selecione o Personagem", 
		"id_ID" => "Pilih Karakter", 
		_ => "Select Character", 
	};

	public static string SelectServer => LocalizeSystem.Locale switch
	{
		"ko_KR" => "서버 선택", 
		"zh_TW" => "選擇伺服器", 
		"th_TH" => "เล\u0e37อกเซ\u0e34ร\u0e4cฟเวอร\u0e4c", 
		"es_MX" => "Seleccionar servidor", 
		"ru_RU" => "Выбрать сервер", 
		"de_DE" => "Server auswählen", 
		"fr_FR" => "Sélectionner le serveur", 
		"pt_BR" => "Selecionar Servidor", 
		"id_ID" => "Pilih Server", 
		_ => "Select Server", 
	};

	public static string MaintainanceAndQuit
	{
		get
		{
			if (Platform.Instance.UsePCUI)
			{
				return Maintainance;
			}
			return Maintainance + "\n" + TapToClose;
		}
	}

	public static string PermissionDeniedAndQuit
	{
		get
		{
			if (Platform.Instance.UsePCUI)
			{
				return PermissionDenied;
			}
			return PermissionDenied + "\n" + TapToClose;
		}
	}

	public static string DataLoadErrorAndRetry
	{
		get
		{
			if (Platform.Instance.UsePCUI)
			{
				return DataLoadError;
			}
			return DataLoadError + "\n" + TapToRetry;
		}
	}

	public static string GetLoginFailedAndRetry(int errorCode)
	{
		if (Platform.Instance.UsePCUI)
		{
			return $"{LoginFailed} ({errorCode})";
		}
		return $"{LoginFailed} ({errorCode})\n{TapToRetry}";
	}

	public static string GetInvalidArguments(int errorCode, int lastErrorFromAccountProvider)
	{
		if (lastErrorFromAccountProvider == 0)
		{
			return $"{InvalidArguments} ({errorCode})";
		}
		return $"{InvalidArguments} ({errorCode} / {lastErrorFromAccountProvider})";
	}

	public static string GetInitializeFailed(int errorCode, int lastErrorFromAccountProvider = 0)
	{
		if (lastErrorFromAccountProvider == 0)
		{
			return $"{InitializeFailed} ({errorCode})";
		}
		return $"{InitializeFailed} ({errorCode} / {lastErrorFromAccountProvider})";
	}
}
