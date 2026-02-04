export const downloadFile = (file: Blob, fileName: string) => {
    if (typeof window === "undefined" || typeof document === "undefined") return;

    const url = URL.createObjectURL(file);
    const link = document.createElement("a");

    link.href = url;
    link.download = fileName;
    link.style.display = "none";

    document.body.appendChild(link);
    link.click();
    link.remove();

    // Delay revoke to avoid rare cases where download hasn't started yet
    setTimeout(() => URL.revokeObjectURL(url), 0);
};
