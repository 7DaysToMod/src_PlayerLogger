using System;

namespace PlayerLogger
{
	public class ConsoleCmdRemovePLBlock:ConsoleCmdAbstract
	{
		public override string GetHelp ()
		{
			return "remplblock <BlockNameOrId> - Tells Playerlogger to track the block <BlockName>";
		}

		public override string GetDescription ()
		{
			return "Removes a block to be tracked by PlayerLogger";
		}

		public override string[] GetCommands ()
		{
			return new string[]{ "remplblock" };
		}

		public override void Execute (System.Collections.Generic.List<string> _params, CommandSenderInfo _senderInfo)
		{
			if (_params.Count != 1) {
				SdtdConsole.Instance.Output ("[PlayerLogger] Invalid params for remplblock");
				return;
			}

			string blockName = _params [0];
			BlockValue? bvn = null;


			int blockId;

			if(int.TryParse(blockName, out blockId)){//checkById
				foreach(Block b in Block.list){
					if (b != null) {
						if (b.blockID == blockId) {
							bvn = Block.GetBlockValue (b.GetBlockName ());
							break;
						}
					}
				}

			}else{ //checkByName
				bvn = Block.GetBlockValue (blockName);
			}

			if (bvn == null) {

				SdtdConsole.Instance.Output ("[PlayerLogger] Invalid Block Name or ID: "+blockName);
				return;
			}

			BlockValue bv = (BlockValue)bvn;


			PlayerLogger.API.Instance.removeTrackedBlock (bv.ToItemValue ().type);
			SdtdConsole.Instance.Output ("Removed Block ID for Logging: "+bv.ToItemValue ().type.ToString());
		}
	}
}

