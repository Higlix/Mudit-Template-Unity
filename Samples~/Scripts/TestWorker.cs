using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using System;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.Enums;

public class TestSpeaker
{
	public void SayYourName()
	{
		Debug.Log("TestSpeaker says: Hello!");
	}
}

public class TestWorker : IDisposable, IStartable
{
	private readonly ISaveService saveService;
	private readonly ISaveSlotManager slotManager;
	private int passed;
	private int failed;

	[Inject]
	public TestWorker(ISaveService saveService, ISaveSlotManager slotManager)
	{
		this.saveService = saveService;
		this.slotManager = slotManager;
		
	}

    void IStartable.Start()
    {
		RunAllTests().Forget();
    }


	public void Dispose()
	{
		Debug.Log("[TestWorker] Disposed");
	}

	private async UniTask RunAllTests()
	{
		// Small delay to let everything settle after boot
		await UniTask.Delay(500);

		Debug.Log("========== SAVE SERVICE TESTS START ==========");

		// --- SaveService Tests ---
		await Test_SaveAndLoad_Plain();
		await Test_SaveAndLoad_Compressed();
		await Test_SaveAndLoad_Encrypted();
		await Test_SaveAndLoad_EncryptedCompressed();
		await Test_SaveableData_Api();
		await Test_ExistsAsync();
		await Test_DeleteAsync();
		await Test_DeleteAllAsync();
		await Test_LoadMissing_ReturnsFail();
		await Test_OverwriteExistingKey();

		// --- SaveSlotManager Tests ---
		await Test_SlotManager_CreateAndExists();
		await Test_SlotManager_SwitchSlot_IsolatesData();
		await Test_SlotManager_DeleteSlot();
		await Test_SlotManager_GetUsedSlots();
		await Test_SlotManager_InvalidSlotThrows();

		Debug.Log($"========== RESULTS: {passed} passed, {failed} failed ==========");
	}

	// ==================== SaveService Tests ====================

	private async UniTask Test_SaveAndLoad_Plain()
	{
		const string testName = "SaveAndLoad_Plain";
		try
		{
			await saveService.DeleteAsync("test_plain");
			var data = new TestData { playerName = "Alice", score = 42 };
			await saveService.SaveAsync("test_plain", data, SaveMode.Plain);

			var result = await saveService.LoadAsync<TestData>("test_plain", SaveMode.Plain);

			Assert(testName, result.Success, "Success should be true");
			Assert(testName, result.Data.playerName == "Alice", $"playerName expected 'Alice', got '{result.Data.playerName}'");
			Assert(testName, result.Data.score == 42, $"score expected 42, got {result.Data.score}");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_plain"); }
	}

	private async UniTask Test_SaveAndLoad_Compressed()
	{
		const string testName = "SaveAndLoad_Compressed";
		try
		{
			await saveService.DeleteAsync("test_compressed");
			var data = new TestData { playerName = "Bob", score = 100 };
			await saveService.SaveAsync("test_compressed", data, SaveMode.Compressed);

			var result = await saveService.LoadAsync<TestData>("test_compressed", SaveMode.Compressed);

			Assert(testName, result.Success, "Success should be true");
			Assert(testName, result.Data.playerName == "Bob", $"playerName expected 'Bob', got '{result.Data.playerName}'");
			Assert(testName, result.Data.score == 100, $"score expected 100, got {result.Data.score}");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_compressed"); }
	}

	private async UniTask Test_SaveAndLoad_Encrypted()
	{
		const string testName = "SaveAndLoad_Encrypted";
		try
		{
			await saveService.DeleteAsync("test_encrypted");
			var data = new TestData { playerName = "Carol", score = 999 };
			await saveService.SaveAsync("test_encrypted", data, SaveMode.Encrypted);

			var result = await saveService.LoadAsync<TestData>("test_encrypted", SaveMode.Encrypted);

			Assert(testName, result.Success, "Success should be true");
			Assert(testName, result.Data.playerName == "Carol", $"playerName expected 'Carol', got '{result.Data.playerName}'");
			Assert(testName, result.Data.score == 999, $"score expected 999, got {result.Data.score}");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_encrypted"); }
	}

	private async UniTask Test_SaveAndLoad_EncryptedCompressed()
	{
		const string testName = "SaveAndLoad_EncryptedCompressed";
		try
		{
			await saveService.DeleteAsync("test_enc_comp");
			var data = new TestData { playerName = "Dave", score = 7777 };
			await saveService.SaveAsync("test_enc_comp", data, SaveMode.EncryptedCompressed);

			var result = await saveService.LoadAsync<TestData>("test_enc_comp", SaveMode.EncryptedCompressed);

			Assert(testName, result.Success, "Success should be true");
			Assert(testName, result.Data.playerName == "Dave", $"playerName expected 'Dave', got '{result.Data.playerName}'");
			Assert(testName, result.Data.score == 7777, $"score expected 7777, got {result.Data.score}");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_enc_comp"); }
	}

	private async UniTask Test_SaveableData_Api()
	{
		const string testName = "SaveableData_Api";
		try
		{
			var data = new TestSaveableData { level = 5, coins = 300 };
			await saveService.DeleteAsync(data.SaveKey);
			await saveService.SaveAsync(data, SaveMode.Plain);

			var result = await saveService.LoadAsync<TestSaveableData>(SaveMode.Plain);

			Assert(testName, result.Success, "Success should be true");
			Assert(testName, result.Data.level == 5, $"level expected 5, got {result.Data.level}");
			Assert(testName, result.Data.coins == 300, $"coins expected 300, got {result.Data.coins}");
			Assert(testName, result.Version == 1, $"version expected 1, got {result.Version}");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_saveable"); }
	}

	private async UniTask Test_ExistsAsync()
	{
		const string testName = "ExistsAsync";
		try
		{
			await saveService.DeleteAsync("test_exists");
			bool before = await saveService.ExistsAsync("test_exists");
			Assert(testName, !before, "Should not exist before save");

			await saveService.SaveAsync("test_exists", new TestData { playerName = "X", score = 1 }, SaveMode.Plain);
			bool after = await saveService.ExistsAsync("test_exists");
			Assert(testName, after, "Should exist after save");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_exists"); }
	}

	private async UniTask Test_DeleteAsync()
	{
		const string testName = "DeleteAsync";
		try
		{
			await saveService.SaveAsync("test_delete", new TestData { playerName = "Y", score = 2 }, SaveMode.Plain);
			bool exists = await saveService.ExistsAsync("test_delete");
			Assert(testName, exists, "Should exist before delete");

			await saveService.DeleteAsync("test_delete");
			bool afterDelete = await saveService.ExistsAsync("test_delete");
			Assert(testName, !afterDelete, "Should not exist after delete");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
	}

	private async UniTask Test_DeleteAllAsync()
	{
		const string testName = "DeleteAllAsync";
		try
		{
			await saveService.SaveAsync("test_all_a", new TestData { playerName = "A", score = 1 }, SaveMode.Plain);
			await saveService.SaveAsync("test_all_b", new TestData { playerName = "B", score = 2 }, SaveMode.Plain);

			await saveService.DeleteAllAsync();

			bool a = await saveService.ExistsAsync("test_all_a");
			bool b = await saveService.ExistsAsync("test_all_b");
			Assert(testName, !a, "File A should not exist after DeleteAll");
			Assert(testName, !b, "File B should not exist after DeleteAll");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
	}

	private async UniTask Test_LoadMissing_ReturnsFail()
	{
		const string testName = "LoadMissing_ReturnsFail";
		try
		{
			await saveService.DeleteAsync("nonexistent_key");
			var result = await saveService.LoadAsync<TestData>("nonexistent_key", SaveMode.Plain);

			Assert(testName, !result.Success, "Loading missing key should return Success=false");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
	}

	private async UniTask Test_OverwriteExistingKey()
	{
		const string testName = "OverwriteExistingKey";
		try
		{
			await saveService.DeleteAsync("test_overwrite");
			await saveService.SaveAsync("test_overwrite", new TestData { playerName = "First", score = 1 }, SaveMode.Plain);
			await saveService.SaveAsync("test_overwrite", new TestData { playerName = "Second", score = 2 }, SaveMode.Plain);

			var result = await saveService.LoadAsync<TestData>("test_overwrite", SaveMode.Plain);

			Assert(testName, result.Success, "Success should be true");
			Assert(testName, result.Data.playerName == "Second", $"Expected 'Second', got '{result.Data.playerName}'");
			Assert(testName, result.Data.score == 2, $"Expected 2, got {result.Data.score}");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally { await saveService.DeleteAsync("test_overwrite"); }
	}

	// ==================== SaveSlotManager Tests ====================

	private async UniTask Test_SlotManager_CreateAndExists()
	{
		const string testName = "SlotManager_CreateAndExists";
		try
		{
			// Clean up first
			if (await slotManager.SlotExistsAsync(1))
				await slotManager.DeleteSlotAsync(1);

			bool before = await slotManager.SlotExistsAsync(1);
			Assert(testName, !before, "Slot 1 should not exist before creation");

			await slotManager.CreateSlotAsync(1);
			bool after = await slotManager.SlotExistsAsync(1);
			Assert(testName, after, "Slot 1 should exist after creation");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally
		{
			try { await slotManager.DeleteSlotAsync(1); } catch { }
		}
	}

	private async UniTask Test_SlotManager_SwitchSlot_IsolatesData()
	{
		const string testName = "SlotManager_SwitchSlot_IsolatesData";
		try
		{
			// Save data in slot 0
			await slotManager.SetActiveSlotAsync(0);
			await saveService.SaveAsync("isolation_test", new TestData { playerName = "Slot0", score = 100 }, SaveMode.Plain);

			// Switch to slot 1 and save different data
			await slotManager.CreateSlotAsync(1);
			await slotManager.SetActiveSlotAsync(1);
			await saveService.SaveAsync("isolation_test", new TestData { playerName = "Slot1", score = 200 }, SaveMode.Plain);

			// Verify slot 1 data
			var result1 = await saveService.LoadAsync<TestData>("isolation_test", SaveMode.Plain);
			Assert(testName, result1.Data.playerName == "Slot1", $"Slot1 expected 'Slot1', got '{result1.Data.playerName}'");

			// Switch back to slot 0 and verify its data is unchanged
			await slotManager.SetActiveSlotAsync(0);
			var result0 = await saveService.LoadAsync<TestData>("isolation_test", SaveMode.Plain);
			Assert(testName, result0.Data.playerName == "Slot0", $"Slot0 expected 'Slot0', got '{result0.Data.playerName}'");

			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally
		{
			try
			{
				await slotManager.SetActiveSlotAsync(0);
				await saveService.DeleteAsync("isolation_test");
				await slotManager.DeleteSlotAsync(1);
			}
			catch { }
		}
	}

	private async UniTask Test_SlotManager_DeleteSlot()
	{
		const string testName = "SlotManager_DeleteSlot";
		try
		{
			await slotManager.CreateSlotAsync(2);
			bool exists = await slotManager.SlotExistsAsync(2);
			Assert(testName, exists, "Slot 2 should exist after creation");

			await slotManager.DeleteSlotAsync(2);
			bool afterDelete = await slotManager.SlotExistsAsync(2);
			Assert(testName, !afterDelete, "Slot 2 should not exist after deletion");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
	}

	private async UniTask Test_SlotManager_GetUsedSlots()
	{
		const string testName = "SlotManager_GetUsedSlots";
		try
		{
			// Slot 0 always exists (active by default). Create slot 2.
			await slotManager.CreateSlotAsync(2);

			int[] used = await slotManager.GetUsedSlotsAsync();
			bool has0 = Array.IndexOf(used, 0) >= 0;
			bool has2 = Array.IndexOf(used, 2) >= 0;
			Assert(testName, has0, "Used slots should include 0");
			Assert(testName, has2, "Used slots should include 2");
			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
		finally
		{
			try { await slotManager.DeleteSlotAsync(2); } catch { }
		}
	}

	private async UniTask Test_SlotManager_InvalidSlotThrows()
	{
		const string testName = "SlotManager_InvalidSlotThrows";
		try
		{
			bool threw = false;
			try
			{
				await slotManager.SetActiveSlotAsync(-1);
			}
			catch (ArgumentOutOfRangeException)
			{
				threw = true;
			}
			Assert(testName, threw, "Setting slot -1 should throw ArgumentOutOfRangeException");

			threw = false;
			try
			{
				await slotManager.SetActiveSlotAsync(slotManager.MaxSlots);
			}
			catch (ArgumentOutOfRangeException)
			{
				threw = true;
			}
			Assert(testName, threw, $"Setting slot {slotManager.MaxSlots} should throw ArgumentOutOfRangeException");

			Pass(testName);
		}
		catch (Exception e) { Fail(testName, e); }
	}

	// ==================== Test Helpers ====================

	private void Assert(string testName, bool condition, string message)
	{
		if (!condition)
			throw new Exception($"Assertion failed: {message}");
	}

	private void Pass(string testName)
	{
		passed++;
		Debug.Log($"  [PASS] {testName}");
	}

	private void Fail(string testName, Exception e)
	{
		failed++;
		Debug.LogError($"  [FAIL] {testName}: {e.Message}");
	}

	// ==================== Test Data Types ====================

	[Serializable]
	private class TestData
	{
		public string playerName;
		public int score;
	}

	[Serializable]
	private class TestSaveableData : ISaveableData
	{
		public int level;
		public int coins;
		public string SaveKey => "test_saveable";
		public int SaveVersion => 1;
	}
}
