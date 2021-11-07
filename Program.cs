using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace S3C
{
	class Program
	{
		//checksum en C#
		private static ushort uiCrc16Cal(byte[] pucY, byte ucX)
		{
			const ushort PRESET_VALUE = 0xFFFF;
			const ushort POLYNOMIAL = 0x8408;

			byte ucI;
			byte ucJ;
			ushort uiCrcValue = PRESET_VALUE;

			for(ucI = 0; ucI < ucX; ucI++)
			{
				uiCrcValue = (ushort)(uiCrcValue ^ pucY[ucI]);
				for(ucJ = 0; ucJ < 8; ucJ++)
				{
					if((uiCrcValue & 0x0001) != 0)
					{
						uiCrcValue = (ushort)((uiCrcValue >> 1) ^ POLYNOMIAL);
					}
					else
					{
						uiCrcValue = (ushort)(uiCrcValue >> 1);
					}
				}
				
			}
			return uiCrcValue;
		}

		static void Main(string[] args)
		{
			//Configuramos y abrimos el puerto
			SerialPort _serialPort = new SerialPort(); 
			_serialPort.PortName = "COM4";
			_serialPort.BaudRate = 115200;
			_serialPort.DtrEnable = true;
			_serialPort.Open();

			//Caracteres de separacion
			int state = 1;
			char[] delimiterChars = { ' ', '=' };
			byte[] Todo = new byte[11];
			byte[] buffer = new byte[6];

			while (true)
			{
				switch (state)
				{
					case 1:

						string stringText = Console.ReadLine();

					
						//Separar los caracteres que se introduzcan
						string[] bloques = stringText.Split(delimiterChars);

						//Arreglo de bytes para cada numero y el operador
						//String --> Float --> Bytes
						byte[] N1 = BitConverter.GetBytes(float.Parse(bloques[0]));
						byte[] N2 = BitConverter.GetBytes(float.Parse(bloques[2]));
						byte[] OP = BitConverter.GetBytes(char.Parse(bloques[1]));
						OP = OP.Where((source, index) => index != 1).ToArray();

						//Todos los bytes en un solo arreglo
						byte[] NumerosOP = N1.ToList().Concat(N2.ToList()).Concat(OP.ToList()).ToArray();
						//Checsum 9bytes (4 n1 - 1 OP - 4 n2 )
						ushort Checksum = uiCrc16Cal(NumerosOP, 9);
						byte[] checkCheck = BitConverter.GetBytes(Checksum);
						Todo = NumerosOP.ToList().Concat(checkCheck.ToList()).ToArray();
						state = 2;
						break;

					case 2:
						//Envia puerto_S los bytesCompletos y checksum
						_serialPort.Write(Todo, 0, 11);
						state = 3;
						break;


					case 3:

						if(_serialPort.BytesToRead >= 6)
						{
							_serialPort.Read(buffer, 0, 6);
							ushort CheckComp = uiCrc16Cal(buffer, 4);
							byte[] bytesCheckComp = BitConverter.GetBytes(CheckComp);
							//Comparacion Checksum Arduino y Checksum C#
							if(buffer[4] == bytesCheckComp[0] && buffer[5] == bytesCheckComp[1])
							{
								Console.Write(BitConverter.ToSingle(buffer, 0));
								Console.WriteLine(" ");
								state = 1;
							}
							else
							{
								state = 1;
							}
						}
						break;

					default:

						break;

						
				}
			}

		}
	}
}
