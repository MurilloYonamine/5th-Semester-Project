# Missions System

Sistema de missões baseado em tipos (enum) com factory pattern. Cada tipo de missão herda de `MissionBase` e define seu próprio comportamento.

## Estrutura

```
Missions/
├── Scripts/
│   ├── IMission.cs              → Interface de contrato para missões
│   ├── MissionBase.cs           → Classe abstrata base
│   ├── CollectItemsMission.cs   → Implementação: coletar N itens
│   ├── CollectAndDeliverMission.cs → Implementação: coletar + entregar
│   ├── MissionFactory.cs        → Factory que cria missões por tipo
│   ├── MissionDefinition.cs     → ScriptableObject (dados)
│   ├── MissionService.cs        → Orquestrador principal
│   ├── SavePoint.cs             → Prefab para checkpoints
│   └── IMissionService.cs       → Interface pública
└── README.md
```

## Tipos de Missão

### CollectItems
Colete X itens de um tipo específico.

**Configuração `MissionDefinition`:**
- `Type` = `CollectItems`
- `TargetItemName` = ex., `"Keycard"`
- `RequiredCount` = ex., 3
- `PersistProgress` = true (salva progresso)

### CollectAndDeliver
Colete X itens e entregue em um ponto específico.

**Configuração `MissionDefinition`:**
- `Type` = `CollectAndDeliver`
- `TargetItemName` = ex., `"Package"`
- `RequiredCount` = ex., 2
- `DeliveryPointId` = ex., `"DeliveryZoneA"`
- `PersistProgress` = true

## Criando uma MissionDefinition

1. Right-click → Create → Gameplay → Mission
2. Preencha os campos:
   - `MissionId` (único, ex.: `mission_collect_keys`)
   - `Title` (ex., `"Collect 3 Keys"`)
   - `Description` (ex., `"Find and collect 3 security keycards"`)
   - `Type` (escolha o tipo)
   - `TargetItemName` (ex., `"Keycard"`)
   - `RequiredCount` (ex., 3)
   - `PersistProgress` (true para salvar progresso)

## Ciclo de Vida

```
1. MissionService inicia
   ├─ Carrega progresso salvo via ISaveService
   ├─ Encontra primeira missão
   └─ Chama SetCurrentMission(0)

2. SetCurrentMission(index)
   ├─ Limpa missão anterior
   ├─ MissionFactory.CreateMission() instancia novo tipo
   ├─ Mission.Initialize(definition, eventBus, saveService)
   ├─ Mission.StartMission()
   └─ Publica MissionUpdatedEvent (UI atualiza)

3. Mission ativa
   ├─ Escuta eventos (InventoryItemAddedEvent, ItemDeliveredEvent)
   ├─ Incrementa Progress
   ├─ Publica MissionProgressEvent a cada mudança
   └─ Chama Complete() ao atingir RequiredCount

4. Complete()
   ├─ Salva progresso via ISaveService
   ├─ Invoca OnMissionComplete event
   ├─ MissionService avança para próxima missão
   └─ Ciclo retorna ao passo 2

5. OnDestroy
   ├─ Cleanup() da missão ativa
   └─ Unsubscribe de eventos
```

## Adicionando um Novo Tipo de Missão

1. Crie classe derivada de `MissionBase` em `Scripts/`

```csharp
public class MyCustomMission : MissionBase {
    public override void StartMission() {
        base.StartMission();
        _eventBus?.Subscribe<MyCustomEvent>(OnCustomEvent);
    }

    private void OnCustomEvent(MyCustomEvent evt) {
        IncrementProgress();
    }

    public override void Cleanup() {
        _eventBus?.Unsubscribe<MyCustomEvent>(OnCustomEvent);
        base.Cleanup();
    }
}
```

2. Adicione tipo novo em `MissionType` enum:

```csharp
public enum MissionType {
    CollectItems = 0,
    CollectAndDeliver = 1,
    MyCustomType = 2
}
```

3. Atualize `MissionFactory.CreateMission()`:

```csharp
public static IMission CreateMission(MissionDefinition definition) {
    GameObject missionGO = new($"{definition.MissionId}_Runtime");
    IMission mission = definition.Type switch {
        MissionType.MyCustomType => missionGO.AddComponent<MyCustomMission>(),
        _ => null
    };
    return mission;
}
```

4. Configure `MissionDefinition` com novo tipo.

## Checkpoints (Save Points)

Crie um GameObject com `SavePoint` component para permitir saves manual:

1. Crie GameObject (ex., `SavePointA`)
2. Adicione componente `SavePoint`
3. Configure `_checkpointId` (ex., `"checkpoint_library"`)
4. Adicione Collider (trigger)
5. Jogador interage → `SavePoint.SaveGame()` grava progresso

### Exemplo de Interação:
```csharp
void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
        savePoint.SaveGame();
    }
}
```

## Persistência

- Progresso é salvo via `ISaveService` (PlayerPrefs) quando:
  - Missão completa (`Complete()` chamado)
  - Checkpoint interagido (`SavePoint.SaveGame()`)

- `MissionBase.Progress` e `MissionProgressEvent.Progress` usam `string`
- Missões baseadas em contagem derivam o número internamente quando precisam comparar com `RequiredCount`
- Progresso é recuperado ao iniciar via `MissionService.Start()` consultando `SaveData`

## UI Integration

`MissionUIController` escuta dois eventos:

1. `MissionUpdatedEvent` → atualiza título/descrição
2. `MissionProgressEvent` → atualiza progress text (ex., "2/5")

Configure no Inspector:
- `_titleText` → UI.Text para título
- `_descriptionText` → UI.Text para descrição
- `_progressText` → UI.Text para progresso

## Debugging

### Ver progresso em Editor:
```csharp
var missionService = ServiceLocator.Get<IMissionService>();
var current = missionService.GetCurrentMission();
Debug.Log($"Current: {current.Title}");
```

### Pular para missão:
```csharp
ServiceLocator.Get<IMissionService>().SkipToMission(2);
```

## Best Practices

- Sempre setei `MissionId` único por missão
- Use `PersistProgress = true` para missões importantes
- Configure `DebugSetupEvents` para testar rápido
- Mantenha `RequiredCount > 0`
- Não modifique `_progress` diretamente; use `IncrementProgress()`
