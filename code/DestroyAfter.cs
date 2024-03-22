using Sandbox;
using System.Threading.Tasks;

public sealed class DestroyAfter : Component
{
	[Property] float UntilDestroy { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_ = DestroyAfterTime();
	}

	async Task DestroyAfterTime()
	{
		await Task.DelaySeconds( UntilDestroy );
		GameObject.Destroy();
	}
}
