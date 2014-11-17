
using UnityEngine;

public   class  BtConnection : MonoBehaviour  {



	private static AndroidJavaClass ajc =new AndroidJavaClass ("com.badran.bluetoothcontroller.Bridge") ;
	//public GameObject = GameObject.
	public static void askEnableBluetooth(){
		ajc.CallStatic ("askEnableBluetooth");
		}
	public static int connect(){
				return ajc.CallStatic<int> ("connect");
		}
	public static bool test () {
		return ajc.CallStatic<bool> ("TESTING");
		}
	//close connection
	public static void close(){
				ajc.CallStatic  ("close");
		}
	//returns true if data there's a data to read
	public static bool available (){
		return ajc.CallStatic <bool> ("available");
		}


	//read from Microcontroller
	public static string read(){
		return ajc.CallStatic<string> ("read");
		}
	//read Control data, for testing
	public static int controlData(){
		return ajc.CallStatic<int>("controlData");
		}

	public static byte [] readBuffer(int length){
		return ajc.CallStatic<byte []>("readBuffer",length);
		}
	public static byte [] readBuffer(int length,byte stopByte){
		return ajc.CallStatic<byte []>("readBuffer",length,stopByte);
		}
	
	public static void sendBytes(byte [] message){
		ajc.CallStatic ("sendBytes", message);
		}

	//send string
	public static void sendString(string message){
		ajc.CallStatic("sendString",message);
		}
	//send 1 char
	public static void sendChar(char message){
		ajc.CallStatic ("sendChar", message);
		}
	//change the default Bluetooth Module name 
	public static void moduleName(string name){
		ajc.CallStatic ("moduleName", name);
		}

	public static void listen(bool start){
		ajc.CallStatic ("listen", start);
		modeNumber = 0;
		}
	public static void listen(bool start,int length){
		ajc.CallStatic ("listen", start,length);
		modeNumber = 1;
		}
	public static void listen(bool start,int length,byte terminalByte){
		ajc.CallStatic ("listen", start,length,terminalByte);
		modeNumber = 2;
	}
	public static void stopListen(){
		ajc.CallStatic ("stopListen");
		}
	public static void moduleMAC(string name){
		ajc.CallStatic ("moduleMac", name);
		}

	public static bool isConnected (){
		return ajc.CallStatic<bool> ("isConnected");
		}
	public static bool isSending (){
		return ajc.CallStatic<bool>("isSending");
		}

	public static bool enableBluetooth(){
		return ajc.CallStatic<bool>("enableBluetooth");
		}


	public static bool isBluetoothEnabled() {
		return ajc.CallStatic<bool>("isBluetoothEnabled");
		}

	public static string readControlData(){
		
		switch(BtConnection.controlData()){
		case 1 : return "Connected"; 
		case 2 : return "Disconnected"; 
		case -1 : return "found your Bluetooth Module but unable to connect to it";
		case -2 : return "Bluetooth module with the name or the MAC you provided can't be found";
		case -3 : return "Connection Failed, usually because your Bluetooth module is off ";
		case -4 : return "error while closing";
		case -5 : return "error while writing";
		case -6 : return "error while reading";
		default : return "Processing...";
		}






	}

	////////////////BtConnector
	/// 
	/// 
	private static int modeNumber = 0;
	public static void doneReading() {
		ajc.CallStatic("doneReading");
	}
	public static int mode(){
		return modeNumber;
	}




}
