using Cysharp.Threading.Tasks;

namespace Mudit.Core.Interfaces.Services
{
	public interface ISaveSlotManager : IRootService
	{
		int ActiveSlot { get; }
		int MaxSlots { get; }
		UniTask SetActiveSlotAsync(int slotIndex);
		UniTask<int[]> GetUsedSlotsAsync();
		UniTask CreateSlotAsync(int slotIndex);
		UniTask DeleteSlotAsync(int slotIndex);
		UniTask<bool> SlotExistsAsync(int slotIndex);
	}
}
