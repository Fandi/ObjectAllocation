namespace Belajaran {
	public enum CoroutinePriority {
		HIGH,
		NORMAL,
		LOW,
		WAITFOR_FIXEDUPDATE,
		WAITFOR_ENDOFFRAME
	}
	
	public enum AllocationState {
		UNALLOCATED,
		ALLOCATING,
		ALLOCATED
	}
	
	public sealed class ObjectAllocation<T>
		where T : UnityEngine.Object {
		public const CoroutinePriority DEFAULT_COROUTINE_PRIORITY = CoroutinePriority.WAITFOR_ENDOFFRAME;
		
		T original;
		T[] allocation;
		
		public ObjectAllocation(T original, int size) {
			if (original == default(T)) {
				throw new UnityEngine.UnassignedReferenceException("Argument 'original' is null");
			}
			
			this.original = original;
			ObjectType = original.GetType();
			
			allocation = new T[size];
			State = AllocationState.UNALLOCATED;
		}
		
		public T this[int index] {
			get {
				return allocation[index];
			}
			set {
				allocation[index] = value;
			}
		}
		
		public int Size {
			get {
				return allocation.Length;
			}
		}
		
		public System.Type ObjectType {
			get;
			private set;
		}
		
		public AllocationState State {
			get;
			private set;
		}
		
		internal UnityEngine.YieldInstruction GetYieldInstruction(CoroutinePriority priority) {
			switch (priority) {
				case CoroutinePriority.HIGH:
					return null;
				case CoroutinePriority.NORMAL:
					return new UnityEngine.WaitForSeconds(0.25f);
				case CoroutinePriority.LOW:
					return new UnityEngine.WaitForSeconds(0.5f);
				case CoroutinePriority.WAITFOR_FIXEDUPDATE:
					return new UnityEngine.WaitForFixedUpdate();
				case CoroutinePriority.WAITFOR_ENDOFFRAME:
					return new UnityEngine.WaitForEndOfFrame();
				default:
					throw new UnityEngine.UnityException("Unhandled CoroutinePriority enum value");
			}
		}
		
		public System.Collections.IEnumerator Pool(System.Action<T> oninstantiated = default(System.Action<T>), System.Action<ObjectAllocation<T>> onallocated = default(System.Action<ObjectAllocation<T>>), CoroutinePriority priority = DEFAULT_COROUTINE_PRIORITY) {
			if (State == AllocationState.ALLOCATED) {
				throw new UnityEngine.UnityException("Is already allocated");
			}
			
			if (State == AllocationState.ALLOCATING) {
				throw new UnityEngine.UnityException("Is allocating");
			}
			
			State = AllocationState.ALLOCATING;
			
			for (int i = 0; i < allocation.Length; i++) {
				allocation[i] = UnityEngine.Object.Instantiate(original) as T;
				
				if (oninstantiated != default(System.Action<T>)) {
					oninstantiated(allocation[i]);
				}
				
				yield return GetYieldInstruction(priority);
			}
			
			State = AllocationState.ALLOCATED;
			
			if (onallocated != default(System.Action<ObjectAllocation<T>>)) {
				onallocated(this);
			}
		}
		
		public System.Collections.IEnumerator Iterate(System.Action<T> oniterating, CoroutinePriority priority = DEFAULT_COROUTINE_PRIORITY) {
			if (State != AllocationState.ALLOCATED) {
				throw new UnityEngine.UnityException("Is not allocated yet");
			}
			
			if (oniterating == default(System.Action<T>)) {
				throw new UnityEngine.UnassignedReferenceException("Argument 'oniterating' is null");
			}
			
			foreach (T obj in allocation) {
				oniterating(obj);
				yield return GetYieldInstruction(priority);
			}
		}
	}
	
	public static class Object {
		public static ObjectAllocation<T> Allocate<T>(T original, int size)
			where T : UnityEngine.Object {
			return new ObjectAllocation<T>(original, size);
		}
	}
}
