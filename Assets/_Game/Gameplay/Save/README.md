# Save System

Sistema de persistência de progresso do jogo usando `PlayerPrefs` com suporte a checkpoints estilo Resident Evil.

Current implementation note:
- The live flow now uses a single autosave slot (`default`).
- Manual save/load screens are no longer part of the active menu flow.
- `SavePoint` is legacy and no longer triggers manual saving.

## Estrutura

```
Save/
├── SaveService.cs      → Implementação do serviço (PlayerPrefs)
├── SavePoint.cs        → Componente para checkpoints interativos
└── README.md
```

## Componentes

### SaveService

Gerencia todos os saves/checkpoints do jogo via `ISaveService`.

**Responsabilidades:**
- Serializar/desserializar `SaveData` em JSON
- Armazenar em slots via `PlayerPrefs`
- Recuperar progresso ao carregar
- Notificar sistemas de quando um save é completado

**Slots de Save:**
- `default` — slot padrão (criado automaticamente)
- `checkpoint_{id}` — slots de checkpoint (ex., `checkpoint_library`)
- `slot_0`, `slot_1`, etc. — slots manuais (para UI de save/load)

**SaveData contém:**
```csharp
public int CurrentMissionIndex;              // Índice da missão ativa
public Dictionary<string, string> MissionProgress; // missionId → progress (string)
public string LastCheckpointId;              // Último checkpoint usado
public int SaveVersion;                      // Versionamento (migrações)
public long Timestamp;                       // Unix timestamp
```

**Registro automático:**
Já registrado em `GameBootstrapper.Initialize()`:
```csharp
var saveService = new SaveService();
ServiceLocator.Register<ISaveService>(saveService);
```

### SavePoint

Componente legado mantido apenas como marcador de checkpoint.

**Setup:**
1. Crie GameObject (ex., `CheckpointLibrary`)
2. Adicione componente `SavePoint`
3. Configure `_checkpointId` (ex., `"checkpoint_library"`)
4. Adicione `Collider` com `isTrigger = true`

**Chamando save:**
```csharp
public void SaveGame() {
    if (_saveService == null || _missionService == null) return;

    SaveData saveData = new() {
        CurrentMissionIndex = _missionService.CurrentIndex
    };

    _saveService.SaveCheckpoint(_checkpointId, saveData);
    Debug.Log($"[SavePoint] Game saved at checkpoint: {_checkpointId}");
}
```

**Exemplo de integração com interação:**
```csharp
void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) {
        savePoint.SaveGame(); // Auto-save ao entrar
    }
}
```

## API

### ISaveService (em Core/Services)

```csharp
// Salvar em um slot
void SaveToSlot(string slotId, SaveData data);

// Carregar de um slot
SaveData LoadFromSlot(string slotId);

// Deletar slot
void DeleteSlot(string slotId);

// Verificar existência
bool SlotExists(string slotId);

// Listar todos os slots
string[] ListSlots();

// Salvar como checkpoint
void SaveCheckpoint(string checkpointId, SaveData data);

// Evento disparado após salvar
event Action<string> OnSaveCompleted;
```

## Uso Típico

### Salvar ao completar missão
```csharp
// Em MissionService ou MissionBase
private void SaveGameState() {
    ISaveService saveService = ServiceLocator.Get<ISaveService>();
    SaveData saveData = new SaveData {
        CurrentMissionIndex = CurrentIndex,
        MissionProgress = GetCurrentProgress() // retorna strings por missão
    };
    saveService.SaveToSlot("default", saveData);
}
```

### Carregar ao iniciar
```csharp
// Em GameBootstrapper ou cena de boot
private void LoadGame() {
    ISaveService saveService = ServiceLocator.Get<ISaveService>();
    SaveData loaded = saveService.LoadFromSlot("default");
    
    if (loaded != null) {
        missionService.SkipToMission(loaded.CurrentMissionIndex);
    }
}
```

### Checkpoint manual
```csharp
// Em SavePoint
public void SaveGame() {
    _saveService.SaveCheckpoint("library_checkpoint", currentSaveData);
}
```

## PlayerPrefs Storage

- **Chave:** `save_{slotId}`
- **Valor:** JSON serializado
- **Exemplos:**
  - `save_default` → save principal
  - `save_checkpoint_library` → checkpoint da biblioteca
  - `save_slot_0` → slot de save manual 0

## Integração com MissionService

`MissionService` já integrado para salvar automaticamente:

```csharp
private void SaveGameState() {
    if (_saveService == null) return;

    SaveData saveData = _saveService.LoadFromSlot("default") ?? new SaveData();
    saveData.CurrentMissionIndex = CurrentIndex;
    _saveService.SaveToSlot("default", saveData);
}
```

Chamado quando:
- Missão completa
- `SkipToMission()` executado

## Persistência de Progresso de Missão

Cada `MissionBase` salva progresso automaticamente quando:
- Inicializa (`LoadProgress()`)
- Completa (`SaveProgress()`)
- Incrementa progresso (se `PersistProgress = true`)

```csharp
protected virtual void SaveProgress() {
    if (!_definition.PersistProgress || _saveService == null) return;

    SaveData saveData = _saveService.LoadFromSlot("default") ?? new SaveData();
    // Progresso de missão é salvo como `string` em `SaveData.MissionProgress`.
    // Missões que precisam de contagem convertem o valor internamente para `int` apenas para comparação com `RequiredCount`.

    saveData.MissionProgress[MissionId] = _progress ?? string.Empty;
    _saveService.SaveToSlot("default", saveData);
}
```

## Versionamento e Migrações

`SaveData.SaveVersion` permite compatibilidade futura:

```csharp
SaveData loaded = saveService.LoadFromSlot("default");
if (loaded.SaveVersion < 2) {
    // Aplicar migração
    loaded = MigrateSaveDataV1ToV2(loaded);
}
```

## Best Practices

- Use `default` slot para progresso principal
- Nomeie checkpoints descritivamente (ex., `checkpoint_library`, `checkpoint_boss_arena`)
- Sempre verificar `SlotExists()` antes de carregar para evitar warnings
- Chamar `SaveGame()` em `SavePoint` via collider ou UI button
- Não salvar em `Update()` — use checkpoints ou eventos de conclusão

## Debugging

### Ver todos os saves:
```csharp
var saveService = ServiceLocator.Get<ISaveService>();
foreach (string slot in saveService.ListSlots()) {
    Debug.Log($"Slot: {slot}");
}
```

### Carregar slot específico:
```csharp
SaveData checkpoint = saveService.LoadFromSlot("checkpoint_library");
Debug.Log($"Mission: {checkpoint.CurrentMissionIndex}");
```

### Limpar saves (para testes):
```csharp
var saveService = ServiceLocator.Get<ISaveService>();
saveService.DeleteSlot("default");
```

## Limitações

- `PlayerPrefs` é visível ao usuário (não seguro)
- Tamanho máximo variável por plataforma (~1MB geralmente)
- Sem encriptação nativa (use salt/hash se necessário)
- Ideal para POC/testes; considere sistema custom para produção

## Próximos Passos

- UI de save/load (exibir slots disponíveis)
- Sistema de auto-save a cada N minutos
- Encriptação de saves (XOR simples ou mais robusta)
- Histórico de saves (timestamps, screenshots)
