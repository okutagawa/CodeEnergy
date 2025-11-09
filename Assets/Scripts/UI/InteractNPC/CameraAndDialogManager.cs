using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CameraAndDialogManager : MonoBehaviour
{
    public static CameraAndDialogManager Instance { get; private set; }

    [Header("Camera")]
    public Camera playerCamera;                // Main Camera
    public Transform cameraRoot;               // родитель камеры (опционально), если камера дочерняя у Player
    public float focusDuration = 0.6f;
    public float returnDuration = 0.6f;

    [Header("Control lock")]
    public MonoBehaviour[] componentsToDisable; // список скриптов управления игроком/камерой (заполнить в инспекторе)

    private Vector3 originalCamPosWorld;
    private Quaternion originalCamRotWorld;
    private Vector3 originalRootLocalPos;
    private Quaternion originalRootLocalRot;
    private bool rootProvided;
    private Coroutine currentRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
        if (playerCamera == null) playerCamera = Camera.main;
        rootProvided = cameraRoot != null;
    }

    void Start()
    {
        SaveOriginalTransforms();
    }

    private void SaveOriginalTransforms()
    {
        if (playerCamera == null) return;
        originalCamPosWorld = playerCamera.transform.position;
        originalCamRotWorld = playerCamera.transform.rotation;
        if (rootProvided)
        {
            originalRootLocalPos = cameraRoot.localPosition;
            originalRootLocalRot = cameraRoot.localRotation;
        }
    }

    public void FocusOnNPC(Transform npcTransform, Vector3 offsetLocalToNpc, float lookAtHeightOffset, Action onFocused = null)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        DisablePlayerControl();
        SaveOriginalTransforms();
        currentRoutine = StartCoroutine(FocusRoutine(npcTransform, offsetLocalToNpc, lookAtHeightOffset, onFocused));
    }

    private IEnumerator FocusRoutine(Transform npcTransform, Vector3 offsetLocalToNpc, float lookAtHeightOffset, Action onFocused)
    {
        Transform camT = playerCamera.transform;

        Vector3 startPos = camT.position;
        Quaternion startRot = camT.rotation;

        // целевая позиция задаётся в локальных координатах NPC, чтобы обеспечить NPC справа/слева по экрану
        Vector3 worldTargetPos = npcTransform.TransformPoint(offsetLocalToNpc);
        worldTargetPos.y += lookAtHeightOffset;

        Vector3 lookAtPoint = npcTransform.position + Vector3.up * lookAtHeightOffset;
        Quaternion targetRot = Quaternion.LookRotation(lookAtPoint - worldTargetPos);

        float t = 0f;
        while (t < focusDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / focusDuration);
            if (rootProvided)
            {
                // если у камеры есть корень, двигаем корень так, чтобы камера в worldTargetPos оказалась корректно
                // рассчитываем нужный world position для root: worldRootPos = worldTargetPos - cameraRoot.TransformVector(localCamPos)
                Vector3 localCamPos = camT.localPosition;
                Vector3 desiredRootPos = worldTargetPos - (cameraRoot.rotation * localCamPos);
                cameraRoot.position = Vector3.Lerp(cameraRoot.position, desiredRootPos, k);
                cameraRoot.rotation = Quaternion.Slerp(cameraRoot.rotation, targetRot, k);
            }
            else
            {
                camT.position = Vector3.Lerp(startPos, worldTargetPos, k);
                camT.rotation = Quaternion.Slerp(startRot, targetRot, k);
            }
            yield return null;
        }

        // окончательно выставляем
        if (rootProvided)
        {
            Vector3 localCamPos = camT.localPosition;
            Vector3 desiredRootPos = worldTargetPos - (cameraRoot.rotation * localCamPos);
            cameraRoot.position = desiredRootPos;
            cameraRoot.rotation = targetRot;
        }
        else
        {
            camT.position = worldTargetPos;
            camT.rotation = targetRot;
        }

        currentRoutine = null;
        onFocused?.Invoke();
    }

    public void ReturnToOriginal(Action onReturned = null)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ReturnRoutine(onReturned));
    }

    private IEnumerator ReturnRoutine(Action onReturned)
    {
        Transform camT = playerCamera.transform;
        Vector3 startPos = camT.position;
        Quaternion startRot = camT.rotation;

        // если есть root, восстанавливаем локальные трансформы root
        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / returnDuration);
            if (rootProvided)
            {
                cameraRoot.position = Vector3.Lerp(cameraRoot.position, originalCamPosWorld - (cameraRoot.rotation * camT.localPosition), k);
                cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, originalRootLocalPos, k); // плавность
                cameraRoot.localRotation = Quaternion.Slerp(cameraRoot.localRotation, originalRootLocalRot, k);
            }
            else
            {
                camT.position = Vector3.Lerp(startPos, originalCamPosWorld, k);
                camT.rotation = Quaternion.Slerp(startRot, originalCamRotWorld, k);
            }
            yield return null;
        }

        if (rootProvided)
        {
            cameraRoot.localPosition = originalRootLocalPos;
            cameraRoot.localRotation = originalRootLocalRot;
            // восстановим world camera для надёжности
            playerCamera.transform.position = originalCamPosWorld;
            playerCamera.transform.rotation = originalCamRotWorld;
        }
        else
        {
            camT.position = originalCamPosWorld;
            camT.rotation = originalCamRotWorld;
        }

        currentRoutine = null;
        EnablePlayerControl();
        onReturned?.Invoke();
    }

    private void DisablePlayerControl()
    {
        // отключаем перечисленные компоненты управления
        if (componentsToDisable == null) return;
        foreach (var c in componentsToDisable)
            if (c != null) c.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnablePlayerControl()
    {
        if (componentsToDisable == null) return;
        foreach (var c in componentsToDisable)
            if (c != null) c.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
