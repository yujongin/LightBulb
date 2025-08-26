using System;
using UnityEngine;

/// <summary>
/// 웹에서 받아올 전체 표-퀴즈 데이터 구조
/// </summary>
[System.Serializable]
public class TableQuizData
{
    public TableInfo tableInfo;
    public TableContent tableContent;
    public QuizDataCollection quizData;
}

/// <summary>
/// 표 기본 정보
/// </summary>
[System.Serializable]
public class TableInfo
{
    public int width;           // 표 너비 (열 개수)
    public int height;          // 표 높이 (행 개수)
}

/// <summary>
/// 표 콘텐츠 데이터
/// </summary>
[System.Serializable]
public class TableContent
{
    public CornerCellData cornerCell;    // (0,0) 코너 셀
    public string[] headerTexts;         // 헤더 행 텍스트들
    public string[] labelTexts;          // 라벨 열 텍스트들
}

/// <summary>
/// 코너 셀 (0,0) 데이터
/// </summary>
[System.Serializable]
public class CornerCellData
{
    public string topText;    // 상단 텍스트 (예: "용질")
    public string bottomText; // 하단 텍스트 (예: "회차")
}

/// <summary>
/// 퀴즈 데이터 컬렉션
/// </summary>
[System.Serializable]
public class QuizDataCollection
{
    public CellQuizData[] cellQuizzes; // 각 셀별 퀴즈 데이터
}

/// <summary>
/// 특정 셀의 퀴즈 데이터
/// </summary>
[System.Serializable]
public class CellQuizData
{
    public int column;              // 셀 열 위치
    public int row;                 // 셀 행 위치
    public WebQuizData quizData;    // 퀴즈 데이터
}

/// <summary>
/// 웹용 퀴즈 데이터 (이미지 URL 포함)
/// </summary>
[System.Serializable]
public class WebQuizData
{
    public string title;
    public QuizType quizType;
    public string[] textChoices;
    public string[] imageChoiceUrls;          // 이미지 선택지 URL들
    public string correctAnswer;              // 인풋 퀴즈용
    public int correctAnswerIndex;
    public bool isCorrectAnswer;              // OX 퀴즈용
    public AnswerResultData answerResult;     // 정답 시 표시할 데이터
}

/// <summary>
/// 정답 시 표시할 결과 데이터
/// </summary>
[System.Serializable]
public class AnswerResultData
{
    public string resultText;     // 정답 시 셀에 표시할 텍스트
    public string resultImageUrl; // 정답 시 셀에 표시할 이미지 URL
}

/// <summary>
/// 웹 데이터를 로컬 시스템 데이터로 변환하는 유틸리티
/// </summary>
public static class WebDataConverter
{
    /// <summary>
    /// WebQuizData를 QuizData로 변환
    /// </summary>
    /// <param name="webQuizData">웹 퀴즈 데이터</param>
    /// <param name="imageSprites">로드된 이미지 스프라이트들</param>
    /// <returns>변환된 QuizData</returns>
    public static QuizData ConvertToQuizData(WebQuizData webQuizData, Sprite[] imageSprites = null)
    {
        if (webQuizData == null)
            return null;

        return new QuizData
        {
            title = webQuizData.title,
            quizType = webQuizData.quizType,
            textChoices = webQuizData.textChoices,
            imageChoices = imageSprites,
            correctAnswer = webQuizData.correctAnswer,
            correctAnswerIndex = webQuizData.correctAnswerIndex,
            isCorrectAnswer = webQuizData.isCorrectAnswer
        };
    }

    /// <summary>
    /// JSON 문자열을 TableQuizData로 파싱
    /// </summary>
    /// <param name="jsonString">JSON 문자열</param>
    /// <returns>파싱된 TableQuizData</returns>
    public static TableQuizData ParseFromJSON(string jsonString)
    {
        try
        {
            return JsonUtility.FromJson<TableQuizData>(jsonString);
        }
        catch (System.Exception e)
        {
            
            return null;
        }
    }

    /// <summary>
    /// TableQuizData를 JSON 문자열로 변환 (테스트용)
    /// </summary>
    /// <param name="data">TableQuizData</param>
    /// <param name="prettyPrint">보기 좋게 포맷할지 여부</param>
    /// <returns>JSON 문자열</returns>
    public static string ConvertToJSON(TableQuizData data, bool prettyPrint = true)
    {
        try
        {
            return JsonUtility.ToJson(data, prettyPrint);
        }
        catch (System.Exception e)
        {
            
            return string.Empty;
        }
    }

    /// <summary>
    /// 테스트용 샘플 데이터 생성
    /// </summary>
    /// <returns>샘플 TableQuizData</returns>
    public static TableQuizData CreateSampleData()
    {
        return new TableQuizData
        {
            tableInfo = new TableInfo
            {
                width = 4,
                height = 6
            },
            tableContent = new TableContent
            {
                cornerCell = new CornerCellData
                {
                    topText = "용질",
                    bottomText = "회차"
                },
                headerTexts = new string[] { "소금", "설탕", "명반" },
                labelTexts = new string[] { "1회차", "2회차", "3회차", "4회차", "5회차" }
            },
            quizData = new QuizDataCollection
            {
                cellQuizzes = new CellQuizData[]
                {
                    new CellQuizData
                    {
                        column = 1,
                        row = 1,
                        quizData = new WebQuizData
                        {
                            title = "<b>소금</b> 용해도 측정",
                            quizType = QuizType.MultipleChoiceText,
                            textChoices = new string[] { "높음", "보통", "낮음", "용해안됨" },
                            imageChoiceUrls = new string[] { },
                            correctAnswer = "",
                            correctAnswerIndex = 0,
                            isCorrectAnswer = false,
                            answerResult = new AnswerResultData
                            {
                                resultText = "O",
                                resultImageUrl = ""
                            }
                        }
                    },
                    new CellQuizData
                    {
                        column = 2,
                        row = 1,
                        quizData = new WebQuizData
                        {
                            title = "설탕 용해도 측정",
                            quizType = QuizType.MultipleChoiceText,
                            textChoices = new string[] { "높음", "보통", "낮음" },
                            imageChoiceUrls = new string[] { },
                            correctAnswer = "",
                            correctAnswerIndex = 1,
                            isCorrectAnswer = false,
                            answerResult = new AnswerResultData
                            {
                                resultText = "O",
                                resultImageUrl = ""
                            }
                        }
                    }
                }
            }
        };
    }
} 