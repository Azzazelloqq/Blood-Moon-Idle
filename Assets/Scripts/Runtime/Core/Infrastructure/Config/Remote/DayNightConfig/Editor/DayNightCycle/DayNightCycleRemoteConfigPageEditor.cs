using Runtime.Core.Infrastructure.Config.Remote.DayNightConfig;
using UnityEditor;
using UnityEngine;

namespace BloodMoonIdle.Editor.DayNightCycle
{
	[CustomEditor(typeof(DayNightCycleRemoteConfigPage))]
	public class DayNightCycleRemoteConfigPageEditor : UnityEditor.Editor
	{
		private SerializedProperty _dayDurationProp;
		private SerializedProperty _nightDurationProp;
		private SerializedProperty _lightingPeriodsProp;
		private SerializedProperty _autoStartProp;
		
		private bool _showDurations = true;
		private bool _showLightingPeriods = true;
		private bool _showSettings = true;
		private bool _compactMode = false;
		
		private int _selectedPeriodIndex = -1;
		private bool _isDragging = false;
		private bool _isDraggingStart = false;

		private void OnEnable()
		{
			_dayDurationProp = serializedObject.FindProperty("_dayDuration");
			_nightDurationProp = serializedObject.FindProperty("_nightDuration");
			_lightingPeriodsProp = serializedObject.FindProperty("_lightingPeriods");
			_autoStartProp = serializedObject.FindProperty("_autoStart");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUILayout.Space(10);
			DrawHeader();
			EditorGUILayout.Space(10);
			
			DrawModeToggle();
			EditorGUILayout.Space(10);
			
			DrawInteractiveTimeline();
			EditorGUILayout.Space(10);
			
			if (!_compactMode)
			{
				DrawDurationSettings();
				EditorGUILayout.Space(10);
				
				DrawLightingPeriods();
				EditorGUILayout.Space(10);
				
				DrawGeneralSettings();
				EditorGUILayout.Space(10);
			}
			else
			{
				DrawCompactSettings();
			}
			
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawHeader()
		{
			var headerStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter
			};
			
			EditorGUILayout.LabelField("Day/Night Cycle Configuration", headerStyle);
			EditorGUILayout.LabelField("Configure timing and lighting for the day/night cycle", EditorStyles.centeredGreyMiniLabel);
		}

		private void DrawModeToggle()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Editor Mode:", GUILayout.Width(100));
			_compactMode = GUILayout.Toggle(_compactMode, "Compact Mode");
			EditorGUILayout.EndHorizontal();
		}

		private void DrawInteractiveTimeline()
		{
			EditorGUILayout.LabelField("Interactive Timeline", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Click and drag period boundaries to adjust timing", MessageType.Info);
			
			var rect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
			rect.height = 50;
			
			HandleTimelineInteraction(rect);
			DrawTimelineBackground(rect);
			DrawTimelinePeriods(rect);
			DrawTimelineLabels(rect);
		}

		private void HandleTimelineInteraction(Rect rect)
		{
			var controlID = GUIUtility.GetControlID(FocusType.Passive);
			var eventType = Event.current.GetTypeForControl(controlID);
			
			switch (eventType)
			{
				case EventType.MouseDown:
					if (rect.Contains(Event.current.mousePosition))
					{
						var normalizedX = (Event.current.mousePosition.x - rect.x) / rect.width;
						var handleInfo = GetHandleAtPosition(normalizedX, rect);
						
						if (handleInfo.periodIndex >= 0)
						{
							// Начинаем перетаскивание ручки
							_selectedPeriodIndex = handleInfo.periodIndex;
							_isDraggingStart = handleInfo.isDraggingStart;
							_isDragging = true;
							
							GUIUtility.hotControl = controlID;
							Event.current.Use();
						}
						else
						{
							// Просто выбираем период для редактирования
							var periodIndex = GetPeriodAtPosition(normalizedX);
							if (periodIndex >= 0)
							{
								_selectedPeriodIndex = periodIndex;
								Event.current.Use();
								Repaint();
							}
						}
					}
					break;
					
				case EventType.MouseDrag:
					if (_isDragging && GUIUtility.hotControl == controlID)
					{
						var normalizedX = Mathf.Clamp01((Event.current.mousePosition.x - rect.x) / rect.width);
						AdjustPeriodWithNeighbors(_selectedPeriodIndex, normalizedX, _isDraggingStart);
						
						Event.current.Use();
						Repaint();
					}
					break;
					
				case EventType.MouseUp:
					if (_isDragging && GUIUtility.hotControl == controlID)
					{
						_isDragging = false;
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
			}
		}

		private (int periodIndex, bool isDraggingStart) GetHandleAtPosition(float normalizedX, Rect rect)
		{
			const float handleSize = 8f; // Размер ручки в пикселях
			var handleSizeNormalized = handleSize / rect.width;
			
			for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
			{
				var period = _lightingPeriodsProp.GetArrayElementAtIndex(i);
				var startTime = period.FindPropertyRelative("_normalizedTimeStart").floatValue;
				var endTime = period.FindPropertyRelative("_normalizedTimeEnd").floatValue;
				
				// Проверяем ручку в начале периода (только если это не первый период)
				if (i > 0 && Mathf.Abs(normalizedX - startTime) <= handleSizeNormalized)
				{
					return (i, true);
				}
				
				// Проверяем ручку в конце периода (только если это не последний период)
				if (i < _lightingPeriodsProp.arraySize - 1 && Mathf.Abs(normalizedX - endTime) <= handleSizeNormalized)
				{
					return (i, false);
				}
			}
			
			return (-1, false);
		}

		private void AdjustPeriodWithNeighbors(int periodIndex, float newValue, bool isDraggingStart)
		{
			var periods = GetSortedPeriods();
			var currentPeriod = _lightingPeriodsProp.GetArrayElementAtIndex(periodIndex);
			
			if (isDraggingStart)
			{
				// Двигаем начало периода
				var prevPeriodIndex = GetPreviousPeriodIndex(periodIndex, periods);
				
				if (prevPeriodIndex >= 0)
				{
					// Подтягиваем конец предыдущего периода
					var prevPeriod = _lightingPeriodsProp.GetArrayElementAtIndex(prevPeriodIndex);
					prevPeriod.FindPropertyRelative("_normalizedTimeEnd").floatValue = newValue;
				}
				
				currentPeriod.FindPropertyRelative("_normalizedTimeStart").floatValue = newValue;
			}
			else
			{
				// Двигаем конец периода
				var nextPeriodIndex = GetNextPeriodIndex(periodIndex, periods);
				
				currentPeriod.FindPropertyRelative("_normalizedTimeEnd").floatValue = newValue;
				
				if (nextPeriodIndex >= 0)
				{
					// Подтягиваем начало следующего периода
					var nextPeriod = _lightingPeriodsProp.GetArrayElementAtIndex(nextPeriodIndex);
					nextPeriod.FindPropertyRelative("_normalizedTimeStart").floatValue = newValue;
				}
			}
		}

		private System.Collections.Generic.List<(int index, float start, float end)> GetSortedPeriods()
		{
			var periods = new System.Collections.Generic.List<(int index, float start, float end)>();
			
			for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
			{
				var period = _lightingPeriodsProp.GetArrayElementAtIndex(i);
				var start = period.FindPropertyRelative("_normalizedTimeStart").floatValue;
				var end = period.FindPropertyRelative("_normalizedTimeEnd").floatValue;
				periods.Add((i, start, end));
			}
			
			periods.Sort((a, b) => a.start.CompareTo(b.start));
			return periods;
		}

		private int GetPreviousPeriodIndex(int currentIndex, System.Collections.Generic.List<(int index, float start, float end)> sortedPeriods)
		{
			var currentStart = _lightingPeriodsProp.GetArrayElementAtIndex(currentIndex).FindPropertyRelative("_normalizedTimeStart").floatValue;
			
			for (int i = 0; i < sortedPeriods.Count; i++)
			{
				if (sortedPeriods[i].index == currentIndex)
				{
					return i > 0 ? sortedPeriods[i - 1].index : -1;
				}
			}
			return -1;
		}

		private int GetNextPeriodIndex(int currentIndex, System.Collections.Generic.List<(int index, float start, float end)> sortedPeriods)
		{
			var currentStart = _lightingPeriodsProp.GetArrayElementAtIndex(currentIndex).FindPropertyRelative("_normalizedTimeStart").floatValue;
			
			for (int i = 0; i < sortedPeriods.Count; i++)
			{
				if (sortedPeriods[i].index == currentIndex)
				{
					return i < sortedPeriods.Count - 1 ? sortedPeriods[i + 1].index : -1;
				}
			}
			return -1;
		}

		private int GetPeriodAtPosition(float normalizedX)
		{
			for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
			{
				var period = _lightingPeriodsProp.GetArrayElementAtIndex(i);
				var startTime = period.FindPropertyRelative("_normalizedTimeStart").floatValue;
				var endTime = period.FindPropertyRelative("_normalizedTimeEnd").floatValue;
				
				if (normalizedX >= startTime && normalizedX <= endTime)
				{
					return i;
				}
			}
			return -1;
		}

		private void DrawTimelineBackground(Rect rect)
		{
			EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1f));
			
			// Рисуем сетку времени
			for (int i = 0; i <= 24; i++)
			{
				var x = rect.x + (i / 24f) * rect.width;
				var lineRect = new Rect(x, rect.y, 1, rect.height);
				EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
			}
		}

		private void DrawTimelinePeriods(Rect rect)
		{
			for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
			{
				var periodProp = _lightingPeriodsProp.GetArrayElementAtIndex(i);
				var startTime = periodProp.FindPropertyRelative("_normalizedTimeStart").floatValue;
				var endTime = periodProp.FindPropertyRelative("_normalizedTimeEnd").floatValue;
				var timeOfDay = (TimeOfDay)periodProp.FindPropertyRelative("_timeOfDay").enumValueIndex;
				var temperature = periodProp.FindPropertyRelative("_temperature").floatValue;
				var filter = periodProp.FindPropertyRelative("_filter").colorValue;
				
				var startX = rect.x + startTime * rect.width;
				var width = (endTime - startTime) * rect.width;
				var periodRect = new Rect(startX, rect.y + 5, width, rect.height - 10);
				
				// Используем реальный цвет из настроек (температура + фильтр)
				var tempColor = GetTemperatureColor(temperature);
				var color = tempColor * filter;
				color.a = 0.7f; // Устанавливаем прозрачность для timeline
				
				if (i == _selectedPeriodIndex)
				{
					color.a = 0.8f;
					EditorGUI.DrawRect(new Rect(periodRect.x - 2, periodRect.y - 2, periodRect.width + 4, periodRect.height + 4), Color.white);
				}
				
				EditorGUI.DrawRect(periodRect, color);
				
				// Рисуем ручки для перетаскивания (только между периодами)
				var handleSize = 6;
				
				// Ручка в начале (только если не первый период)
				if (i > 0)
				{
					var startHandle = new Rect(startX - handleSize/2, rect.y, handleSize, rect.height);
					EditorGUI.DrawRect(startHandle, Color.white);
				}
				
				// Ручка в конце (только если не последний период)
				if (i < _lightingPeriodsProp.arraySize - 1)
				{
					var endHandle = new Rect(startX + width - handleSize/2, rect.y, handleSize, rect.height);
					EditorGUI.DrawRect(endHandle, Color.white);
				}
			}
		}

		private void DrawTimelineHandles(Rect rect)
		{
			// Эта функция больше не нужна, так как ручки рисуются в DrawTimelinePeriods
		}

		private void DrawTimelineLabels(Rect rect)
		{
			var labelStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleCenter
			};
			
			for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
			{
				var periodProp = _lightingPeriodsProp.GetArrayElementAtIndex(i);
				var startTime = periodProp.FindPropertyRelative("_normalizedTimeStart").floatValue;
				var endTime = periodProp.FindPropertyRelative("_normalizedTimeEnd").floatValue;
				var timeOfDay = (TimeOfDay)periodProp.FindPropertyRelative("_timeOfDay").enumValueIndex;
				
				var centerX = rect.x + (startTime + endTime) * 0.5f * rect.width;
				var labelRect = new Rect(centerX - 30, rect.y + rect.height + 2, 60, 15);
				
				GUI.Label(labelRect, timeOfDay.ToString(), labelStyle);
			}
		}

		private void DrawCompactSettings()
		{
			if (_selectedPeriodIndex >= 0 && _selectedPeriodIndex < _lightingPeriodsProp.arraySize)
			{
				EditorGUILayout.LabelField("Selected Period Settings", EditorStyles.boldLabel);
				DrawLightingPeriod(_lightingPeriodsProp.GetArrayElementAtIndex(_selectedPeriodIndex), _selectedPeriodIndex);
			}
			else
			{
				EditorGUILayout.HelpBox("Select a period on the timeline to edit its properties", MessageType.Info);
			}
			
			EditorGUILayout.Space(10);
			DrawGeneralSettings();
		}

		private void DrawDurationSettings()
		{
			_showDurations = EditorGUILayout.Foldout(_showDurations, "Duration Settings", true);
			
			if (_showDurations)
			{
				EditorGUI.indentLevel++;
				
				EditorGUILayout.BeginVertical("box");
				
				// Day Duration
				EditorGUILayout.LabelField("Day Duration", EditorStyles.boldLabel);
				DrawTimeProperty(_dayDurationProp);
				DrawTimePresets(_dayDurationProp, "Day");
				
				EditorGUILayout.Space(10);
				
				// Night Duration
				EditorGUILayout.LabelField("Night Duration", EditorStyles.boldLabel);
				DrawTimeProperty(_nightDurationProp);
				DrawTimePresets(_nightDurationProp, "Night");
				
				EditorGUILayout.EndVertical();
				
				// Total cycle info
				var dayTime = GetTimeFromProperty(_dayDurationProp);
				var nightTime = GetTimeFromProperty(_nightDurationProp);
				var totalTime = dayTime + nightTime;
				
				EditorGUILayout.Space(5);
				var infoStyle = new GUIStyle(EditorStyles.helpBox);
				EditorGUILayout.BeginVertical(infoStyle);
				EditorGUILayout.LabelField("Cycle Summary", EditorStyles.boldLabel);
				EditorGUILayout.LabelField($"Day: {FormatTime(dayTime)}", EditorStyles.miniLabel);
				EditorGUILayout.LabelField($"Night: {FormatTime(nightTime)}", EditorStyles.miniLabel);
				EditorGUILayout.LabelField($"Total: {FormatTime(totalTime)}", EditorStyles.miniLabel);
				EditorGUILayout.EndVertical();
				
				EditorGUI.indentLevel--;
			}
		}

		private void DrawTimePresets(SerializedProperty timeProperty, string label)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"{label} Presets:", GUILayout.Width(80));
			
			if (GUILayout.Button("30s", GUILayout.Width(40)))
				SetTimeProperty(timeProperty, 0, 0, 30);
			if (GUILayout.Button("1m", GUILayout.Width(40)))
				SetTimeProperty(timeProperty, 0, 1, 0);
			if (GUILayout.Button("2m", GUILayout.Width(40)))
				SetTimeProperty(timeProperty, 0, 2, 0);
			if (GUILayout.Button("5m", GUILayout.Width(40)))
				SetTimeProperty(timeProperty, 0, 5, 0);
			if (GUILayout.Button("10m", GUILayout.Width(40)))
				SetTimeProperty(timeProperty, 0, 10, 0);
			
			EditorGUILayout.EndHorizontal();
		}

		private void SetTimeProperty(SerializedProperty timeProperty, int hours, int minutes, int seconds)
		{
			timeProperty.FindPropertyRelative("_hours").intValue = hours;
			timeProperty.FindPropertyRelative("_minutes").intValue = minutes;
			timeProperty.FindPropertyRelative("_seconds").intValue = seconds;
		}

		private string FormatTime(float totalSeconds)
		{
			var hours = (int)(totalSeconds / 3600);
			var minutes = (int)((totalSeconds % 3600) / 60);
			var seconds = (int)(totalSeconds % 60);
			
			if (hours > 0)
				return $"{hours}h {minutes}m {seconds}s";
			else if (minutes > 0)
				return $"{minutes}m {seconds}s";
			else
				return $"{seconds}s";
		}

		private void DrawTimeProperty(SerializedProperty timeProperty)
		{
			var hoursProp = timeProperty.FindPropertyRelative("_hours");
			var minutesProp = timeProperty.FindPropertyRelative("_minutes");
			var secondsProp = timeProperty.FindPropertyRelative("_seconds");
			
			EditorGUILayout.BeginVertical();
			
			// Показываем общее время
			var totalSeconds = hoursProp.intValue * 3600 + minutesProp.intValue * 60 + secondsProp.intValue;
			EditorGUILayout.LabelField($"Total: {totalSeconds} seconds ({totalSeconds / 60f:F1} minutes)", EditorStyles.miniLabel);
			
			EditorGUILayout.Space(3);
			
			// Формат данных
			EditorGUILayout.LabelField("Format: hh:mm:ss", EditorStyles.centeredGreyMiniLabel);
			
			// Поля ввода в одну строку
			EditorGUILayout.BeginHorizontal();
			
			// Часы
			hoursProp.intValue = Mathf.Clamp(EditorGUILayout.IntField(hoursProp.intValue, GUILayout.Width(35)), 0, 23);
			
			// Минуты  
			minutesProp.intValue = Mathf.Clamp(EditorGUILayout.IntField(minutesProp.intValue, GUILayout.Width(35)), 0, 59);
			
			// Секунды
			secondsProp.intValue = Mathf.Clamp(EditorGUILayout.IntField(secondsProp.intValue, GUILayout.Width(35)), 0, 59);
			
			GUILayout.FlexibleSpace();
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.EndVertical();
		}

		private void DrawLightingPeriods()
		{
			_showLightingPeriods = EditorGUILayout.Foldout(_showLightingPeriods, "Lighting Periods", true);
			
			if (_showLightingPeriods)
			{
				EditorGUI.indentLevel++;
				
				// Валидация и автосортировка
				ValidateAndSortPeriods();
				
				for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
				{
					DrawLightingPeriod(_lightingPeriodsProp.GetArrayElementAtIndex(i), i);
					EditorGUILayout.Space(5);
				}
				
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Add Lighting Period"))
				{
					AddNewPeriod();
				}
				
				if (GUILayout.Button("Reset to Defaults"))
				{
					ResetLightingPeriodsToDefaults();
				}
				
				if (GUILayout.Button("Auto-Fix Gaps"))
				{
					AutoFixGapsAndOverlaps();
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUI.indentLevel--;
			}
		}

		private void ValidateAndSortPeriods()
		{
			// Проверяем, что все периоды имеют корректные значения
			for (int i = 0; i < _lightingPeriodsProp.arraySize; i++)
			{
				var period = _lightingPeriodsProp.GetArrayElementAtIndex(i);
				var start = period.FindPropertyRelative("_normalizedTimeStart");
				var end = period.FindPropertyRelative("_normalizedTimeEnd");
				
				// Убеждаемся, что start < end
				if (start.floatValue >= end.floatValue)
				{
					end.floatValue = Mathf.Min(start.floatValue + 0.01f, 1f);
				}
				
				// Ограничиваем значения от 0 до 1
				start.floatValue = Mathf.Clamp01(start.floatValue);
				end.floatValue = Mathf.Clamp01(end.floatValue);
			}
		}

		private void AddNewPeriod()
		{
			_lightingPeriodsProp.arraySize++;
			var newIndex = _lightingPeriodsProp.arraySize - 1;
			var newPeriod = _lightingPeriodsProp.GetArrayElementAtIndex(newIndex);
			
			// Находим свободное место для нового периода
			var gaps = FindGaps();
			if (gaps.Count > 0)
			{
				var gap = gaps[0];
				SetLightingPeriod(newIndex, TimeOfDay.Day, 5500f, Color.white, 1f, gap.start, gap.end);
			}
			else
			{
				// Если нет пробелов, добавляем в конец
				var lastEnd = GetLastPeriodEnd();
				SetLightingPeriod(newIndex, TimeOfDay.Day, 5500f, Color.white, 1f, lastEnd, Mathf.Min(lastEnd + 0.1f, 1f));
			}
		}

		private System.Collections.Generic.List<(float start, float end)> FindGaps()
		{
			var gaps = new System.Collections.Generic.List<(float start, float end)>();
			var sortedPeriods = GetSortedPeriods();
			
			if (sortedPeriods.Count == 0)
			{
				gaps.Add((0f, 1f));
				return gaps;
			}
			
			// Проверяем пробел в начале
			if (sortedPeriods[0].start > 0f)
			{
				gaps.Add((0f, sortedPeriods[0].start));
			}
			
			// Проверяем пробелы между периодами
			for (int i = 0; i < sortedPeriods.Count - 1; i++)
			{
				var currentEnd = sortedPeriods[i].end;
				var nextStart = sortedPeriods[i + 1].start;
				
				if (nextStart > currentEnd + 0.001f)
				{
					gaps.Add((currentEnd, nextStart));
				}
			}
			
			// Проверяем пробел в конце
			var lastEnd = sortedPeriods[sortedPeriods.Count - 1].end;
			if (lastEnd < 1f)
			{
				gaps.Add((lastEnd, 1f));
			}
			
			return gaps;
		}

		private float GetLastPeriodEnd()
		{
			var sortedPeriods = GetSortedPeriods();
			return sortedPeriods.Count > 0 ? sortedPeriods[sortedPeriods.Count - 1].end : 0f;
		}

		private void AutoFixGapsAndOverlaps()
		{
			var sortedPeriods = GetSortedPeriods();
			if (sortedPeriods.Count == 0) return;
			
			// Равномерно распределяем периоды по всему диапазону 0-1
			var segmentSize = 1f / sortedPeriods.Count;
			
			for (int i = 0; i < sortedPeriods.Count; i++)
			{
				var period = _lightingPeriodsProp.GetArrayElementAtIndex(sortedPeriods[i].index);
				var start = i * segmentSize;
				var end = (i + 1) * segmentSize;
				
				period.FindPropertyRelative("_normalizedTimeStart").floatValue = start;
				period.FindPropertyRelative("_normalizedTimeEnd").floatValue = end;
			}
		}

		private void DrawLightingPeriod(SerializedProperty periodProp, int index)
		{
			var timeOfDayProp = periodProp.FindPropertyRelative("_timeOfDay");
			var temperatureProp = periodProp.FindPropertyRelative("_temperature");
			var filterProp = periodProp.FindPropertyRelative("_filter");
			var intensityProp = periodProp.FindPropertyRelative("_intensity");
			var startTimeProp = periodProp.FindPropertyRelative("_normalizedTimeStart");
			var endTimeProp = periodProp.FindPropertyRelative("_normalizedTimeEnd");
			
			var timeOfDay = (TimeOfDay)timeOfDayProp.enumValueIndex;
			var headerColor = GetTimeOfDayColor(timeOfDay);
			
			var originalBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = headerColor;
			
			EditorGUILayout.BeginVertical("box");
			GUI.backgroundColor = originalBackgroundColor;
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"{timeOfDay} Period", EditorStyles.boldLabel);
			
			if (GUILayout.Button("X", GUILayout.Width(20)))
			{
				_lightingPeriodsProp.DeleteArrayElementAtIndex(index);
				return;
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUI.indentLevel++;
			
			EditorGUILayout.PropertyField(timeOfDayProp, new GUIContent("Time of Day"));
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Time Range", GUILayout.Width(120));
			var start = EditorGUILayout.Slider(startTimeProp.floatValue, 0f, 1f);
			EditorGUILayout.LabelField("to", GUILayout.Width(20));
			var end = EditorGUILayout.Slider(endTimeProp.floatValue, 0f, 1f);
			EditorGUILayout.EndHorizontal();
			
			// Проверяем, изменились ли значения, и применяем логику подтягивания соседей
			if (start != startTimeProp.floatValue)
			{
				AdjustPeriodWithNeighbors(index, start, true);
			}
			else if (end != endTimeProp.floatValue)
			{
				AdjustPeriodWithNeighbors(index, end, false);
			}
			
			EditorGUILayout.Slider(temperatureProp, 1000f, 20000f, "Temperature (K)");
			EditorGUILayout.PropertyField(filterProp, new GUIContent("Color Filter"));
			EditorGUILayout.Slider(intensityProp, 0f, 8f, "Intensity");
			
			var tempColor = GetTemperatureColor(temperatureProp.floatValue);
			var finalColor = tempColor * filterProp.colorValue;
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Color Preview:", GUILayout.Width(120));
			var previewRect = GUILayoutUtility.GetRect(100, 20);
			EditorGUI.DrawRect(previewRect, finalColor);
			EditorGUILayout.EndHorizontal();
			
			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
		}

		private void DrawGeneralSettings()
		{
			_showSettings = EditorGUILayout.Foldout(_showSettings, "General Settings", true);
			
			if (_showSettings)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(_autoStartProp, new GUIContent("Auto Start Cycle"));
				EditorGUI.indentLevel--;
			}
		}

		private Color GetTimeOfDayColor(TimeOfDay timeOfDay)
		{
			switch (timeOfDay)
			{
				case TimeOfDay.Dawn: return new Color(1f, 0.8f, 0.6f, 0.7f);
				case TimeOfDay.Day: return new Color(1f, 1f, 0.8f, 0.7f);
				case TimeOfDay.Noon: return new Color(1f, 1f, 1f, 0.7f);
				case TimeOfDay.Dusk: return new Color(1f, 0.6f, 0.3f, 0.7f);
				case TimeOfDay.Night: return new Color(0.3f, 0.4f, 0.8f, 0.7f);
				case TimeOfDay.Midnight: return new Color(0.2f, 0.2f, 0.4f, 0.7f);
				default: return Color.white;
			}
		}

		private Color GetTemperatureColor(float temperature)
		{
			var normalizedTemp = (temperature - 1000f) / 19000f;
			
			if (normalizedTemp < 0.5f)
			{
				return Color.Lerp(new Color(1f, 0.3f, 0f), Color.white, normalizedTemp * 2f);
			}
			else
			{
				return Color.Lerp(Color.white, new Color(0.7f, 0.8f, 1f), (normalizedTemp - 0.5f) * 2f);
			}
		}

		private float GetTimeFromProperty(SerializedProperty timeProperty)
		{
			var hours = timeProperty.FindPropertyRelative("_hours").intValue;
			var minutes = timeProperty.FindPropertyRelative("_minutes").intValue;
			var seconds = timeProperty.FindPropertyRelative("_seconds").intValue;
			
			return hours * 3600f + minutes * 60f + seconds;
		}

		private void ResetLightingPeriodsToDefaults()
		{
			_lightingPeriodsProp.arraySize = 6;
			
			SetLightingPeriod(0, TimeOfDay.Dawn, 3500f, Color.white, 0.5f, 0f, 0.125f);
			SetLightingPeriod(1, TimeOfDay.Day, 5500f, Color.white, 1f, 0.125f, 0.375f);
			SetLightingPeriod(2, TimeOfDay.Noon, 6500f, Color.white, 1.2f, 0.375f, 0.625f);
			SetLightingPeriod(3, TimeOfDay.Dusk, 3000f, new Color(1f, 0.6f, 0.3f, 1f), 0.7f, 0.625f, 0.75f);
			SetLightingPeriod(4, TimeOfDay.Night, 2000f, new Color(0.3f, 0.4f, 0.8f, 1f), 0.2f, 0.75f, 0.875f);
			SetLightingPeriod(5, TimeOfDay.Midnight, 1500f, new Color(0.2f, 0.3f, 0.6f, 1f), 0.1f, 0.875f, 1f);
		}

		private void SetLightingPeriod(int index, TimeOfDay timeOfDay, float temperature, Color filter, float intensity, float start, float end)
		{
			var periodProp = _lightingPeriodsProp.GetArrayElementAtIndex(index);
			
			periodProp.FindPropertyRelative("_timeOfDay").enumValueIndex = (int)timeOfDay;
			periodProp.FindPropertyRelative("_temperature").floatValue = temperature;
			periodProp.FindPropertyRelative("_filter").colorValue = filter;
			periodProp.FindPropertyRelative("_intensity").floatValue = intensity;
			periodProp.FindPropertyRelative("_normalizedTimeStart").floatValue = start;
			periodProp.FindPropertyRelative("_normalizedTimeEnd").floatValue = end;
		}
	}
} 