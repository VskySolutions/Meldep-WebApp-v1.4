<template>
    <button id="chat-toggle-btn" aria-label="Open chat" @click="toggleWindow">
        <svg v-if="!isOpen" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round"
            stroke-linejoin="round">
            <path
                d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z" />
        </svg>

        <svg v-else viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round"
            stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
        </svg>
    </button>

    <div id="chat-window" :class="{ open: isOpen }">
        <div class="chat-header">
            <div class="title-block">
                <div class="avatar">
                    <img src="https://meldep.com/assets/logo-BArypmoQ.png" alt="logo" width="40" height="40" />
                </div>

                <div>
                    <div class="title">SOP Assistant</div>
                    <div class="subtitle">Ask about Standard Operating Procedures</div>
                </div>
            </div>

            <div class="actions">
                <button class="icon-btn" title="Minimize" @click="toggleWindow">
                    <svg viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round">
                        <line x1="5" y1="12" x2="19" y2="12" />
                    </svg>
                </button>
            </div>
        </div>

        <div class="chat-toolbar">
            <button title="Start a new conversation" @click="newChat">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round">
                    <line x1="12" y1="5" x2="12" y2="19" />
                    <line x1="5" y1="12" x2="19" y2="12" />
                </svg>
                New Chat
            </button>

            <button title="Clear this conversation" @click="clearChat">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round">
                    <polyline points="3 6 5 6 21 6" />
                    <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
                    <path d="M10 11v6" />
                    <path d="M14 11v6" />
                    <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
                </svg>
                Clear
            </button>
        </div>

        <div ref="messagesEl" class="chat-messages">
            <div v-for="msg in messages" :key="msg.id" class="msg-row" :class="msg.role">
                <div class="bubble" :class="msg.role"
                    v-html="msg.role === 'bot' ? renderMarkdown(msg.text) : escapeHtml(msg.text)"></div>
            </div>

            <div v-if="isTyping" class="msg-row bot">
                <div class="typing">
                    <span></span>
                    <span></span>
                    <span></span>
                </div>
            </div>
        </div>

        <div class="chat-input-row">
            <input v-model="chatInput" type="text" placeholder="Type your question..." autocomplete="off"
                @keydown.enter="sendMessage" />

            <button class="send-btn" aria-label="Send" :disabled="isSending" @click="sendMessage">
                <svg viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round"
                    stroke-linejoin="round">
                    <line x1="22" y1="2" x2="11" y2="13" />
                    <polygon points="22 2 15 22 11 13 2 9 22 2" />
                </svg>
            </button>
        </div>
    </div>
</template>

<script setup>
import { nextTick, ref } from 'vue'

const WEBHOOK_URL =
    'https://n8nworkflow.vskyapplications.com/webhook/c9cb3684-587b-4a09-97f9-b3bb4a0b357f/chat'

const isOpen = ref(false)
const isSending = ref(false)
const isTyping = ref(false)
const chatInput = ref('')
const messages = ref([])
const messagesEl = ref(null)
const sessionId = ref(crypto.randomUUID())

function toggleWindow() {
    isOpen.value = !isOpen.value

    if (isOpen.value && messages.value.length === 0) {
        addBotMessage(
            "Hi! I'm the SOP Assistant. Ask me anything about your Standard Operating Procedures."
        )
    }
}

function addUserMessage(text) {
    messages.value.push({
        id: crypto.randomUUID(),
        role: 'user',
        text,
    })

    scrollToBottom()
}

function addBotMessage(text) {
    messages.value.push({
        id: crypto.randomUUID(),
        role: 'bot',
        text,
    })

    scrollToBottom()
}

function escapeHtml(text = '') {
    return String(text)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;')
}

function renderMarkdown(text = '') {
    let html = escapeHtml(text)

    html = html
        .replace(/^### (.*$)/gim, '<h3>$1</h3>')
        .replace(/^## (.*$)/gim, '<h2>$1</h2>')
        .replace(/^# (.*$)/gim, '<h1>$1</h1>')
        .replace(/\*\*(.*?)\*\*/gim, '<strong>$1</strong>')
        .replace(/\*(.*?)\*/gim, '<em>$1</em>')
        .replace(/`([^`]+)`/gim, '<code>$1</code>')
        .replace(/\n/g, '<br />')

    return html
}

async function scrollToBottom() {
    await nextTick()

    if (messagesEl.value) {
        messagesEl.value.scrollTop = messagesEl.value.scrollHeight
    }
}

async function sendMessage() {
    const text = chatInput.value.trim()

    if (!text || isSending.value) {
        return
    }

    addUserMessage(text)

    chatInput.value = ''
    isSending.value = true
    isTyping.value = true

    try {
        const response = await fetch(WEBHOOK_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                action: 'sendMessage',
                sessionId: sessionId.value,
                chatInput: text,
            }),
        })

        const data = await response.json()

        const reply =
            data.output ||
            data.text ||
            data.reply ||
            data.message ||
            (typeof data === 'string' ? data : JSON.stringify(data))

        addBotMessage(reply)
    } catch (error) {
        addBotMessage(
            "Couldn't reach the assistant. Please check the webhook URL or your connection."
        )
        console.error(error)
    } finally {
        isTyping.value = false
        isSending.value = false
    }
}

function newChat() {
    sessionId.value = crypto.randomUUID()
    messages.value = []
    chatInput.value = ''
    isTyping.value = false
    isSending.value = false

    addBotMessage('Hi! Starting a new conversation. Ask me anything about your SOPs!')
}

function clearChat() {
    messages.value = []
    chatInput.value = ''
    isTyping.value = false
    isSending.value = false
}
</script>

<style scoped>
:global(:root) {
    --meld-navy: #0d2436;
    --meld-blue: #1f6fad;
    --meld-blue-dark: #17587f;
    --meld-blue-light: #eaf3fa;
    --bg: #f3f6f9;
    --text-dark: #1c2733;
    --text-muted: #6b7684;
    --radius: 16px;
}

* {
    box-sizing: border-box;
}

.page {
    text-align: center;
    max-width: 480px;
    padding: 40px;
    margin: 0 auto;
}

.page .icon {
    width: 64px;
    height: 64px;
    border-radius: 18px;
    background: linear-gradient(135deg, var(--meld-blue), var(--meld-navy));
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 20px;
    font-size: 30px;
}

.page h1 {
    font-size: 24px;
    color: var(--text-dark);
    margin-bottom: 8px;
}

.page p {
    color: var(--text-muted);
    font-size: 15px;
    line-height: 1.5;
}

#chat-toggle-btn {
    position: fixed;
    bottom: 28px;
    right: 28px;
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: linear-gradient(135deg, var(--meld-blue), var(--meld-navy));
    color: white;
    border: none;
    cursor: pointer;
    box-shadow: 0 6px 20px rgba(13, 36, 54, 0.45);
    z-index: 9999;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: transform 0.2s ease;
}

#chat-toggle-btn:hover {
    transform: scale(1.07);
}

#chat-toggle-btn svg {
    width: 28px;
    height: 28px;
}

#chat-window {
    position: fixed;
    bottom: 108px;
    right: 28px;
    width: 480px;
    height: 800px;
    max-height: 86vh;
    background: var(--bg);
    border-radius: var(--radius);
    box-shadow: 0 14px 44px rgba(0, 0, 0, 0.25);
    display: none;
    flex-direction: column;
    overflow: hidden;
    z-index: 9999;
    border: 1px solid rgba(0, 0, 0, 0.06);
}

#chat-window.open {
    display: flex;
}

.chat-header {
    background: linear-gradient(135deg, var(--meld-navy), var(--meld-blue-dark));
    color: white;
    padding: 18px 20px;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.title-block {
    display: flex;
    align-items: center;
    gap: 12px;
}

.avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background: rgba(255, 255, 255, 0.15);
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
}

.title {
    font-size: 16.5px;
    font-weight: 700;
    letter-spacing: 0.2px;
}

.subtitle {
    font-size: 12px;
    opacity: 0.85;
    margin-top: 2px;
}

.actions {
    display: flex;
    align-items: center;
    gap: 6px;
}

.icon-btn {
    background: rgba(255, 255, 255, 0.15);
    border: none;
    color: white;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
}

.icon-btn:hover {
    background: rgba(255, 255, 255, 0.28);
}

.icon-btn svg {
    width: 16px;
    height: 16px;
}

.chat-toolbar {
    display: flex;
    gap: 10px;
    padding: 12px 16px;
    background: white;
    border-bottom: 1px solid #e3e8ee;
}

.chat-toolbar button {
    flex: 1;
    font-size: 13px;
    font-weight: 600;
    padding: 9px 12px;
    border-radius: 9px;
    border: 1px solid #cfe1ee;
    background: var(--meld-blue-light);
    color: var(--meld-blue-dark);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
}

.chat-toolbar button:hover {
    background: #d9ecf8;
}

.chat-toolbar button svg {
    width: 14px;
    height: 14px;
}

.chat-messages {
    flex: 1;
    overflow-y: auto;
    padding: 18px;
    display: flex;
    flex-direction: column;
    gap: 14px;
}

.msg-row {
    display: flex;
}

.msg-row.user {
    justify-content: flex-end;
}

.msg-row.bot {
    justify-content: flex-start;
}

.bubble {
    max-width: 82%;
    padding: 12px 16px;
    border-radius: 14px;
    font-size: 14.5px;
    line-height: 1.55;
    word-wrap: break-word;
}

.bubble.user {
    background: var(--meld-blue);
    color: white;
    border-bottom-right-radius: 4px;
}

.bubble.bot {
    background: white;
    color: var(--text-dark);
    border: 1px solid #e3e8ee;
    border-bottom-left-radius: 4px;
}

.bubble.bot :deep(h1),
.bubble.bot :deep(h2),
.bubble.bot :deep(h3) {
    font-size: 15px;
    margin: 6px 0 6px;
    color: var(--meld-navy);
    font-weight: 700;
}

.bubble.bot :deep(p) {
    margin: 0 0 8px;
}

.bubble.bot :deep(strong) {
    color: var(--meld-navy);
    font-weight: 700;
}

.bubble.bot :deep(code) {
    background: var(--meld-blue-light);
    color: var(--meld-blue-dark);
    padding: 2px 5px;
    border-radius: 4px;
    font-size: 13px;
}

.typing {
    display: flex;
    gap: 4px;
    padding: 13px 16px;
    background: white;
    border: 1px solid #e3e8ee;
    border-radius: 14px;
    border-bottom-left-radius: 4px;
    width: fit-content;
}

.typing span {
    width: 6px;
    height: 6px;
    background: #b7c2cc;
    border-radius: 50%;
    animation: bounce 1.2s infinite ease-in-out;
}

.typing span:nth-child(2) {
    animation-delay: 0.15s;
}

.typing span:nth-child(3) {
    animation-delay: 0.3s;
}

@keyframes bounce {

    0%,
    60%,
    100% {
        transform: translateY(0);
        opacity: 0.5;
    }

    30% {
        transform: translateY(-4px);
        opacity: 1;
    }
}

.chat-input-row {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 14px;
    background: white;
    border-top: 1px solid #e3e8ee;
}

.chat-input-row input {
    flex: 1;
    border: 1px solid #d5dee6;
    background: var(--bg);
    padding: 12px 16px;
    border-radius: 22px;
    font-size: 14px;
    outline: none;
}

.chat-input-row input:focus {
    border-color: var(--meld-blue);
}

.send-btn {
    width: 42px;
    height: 42px;
    border-radius: 50%;
    border: none;
    background: var(--meld-blue);
    color: white;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}

.send-btn:hover {
    background: var(--meld-blue-dark);
}

.send-btn svg {
    width: 17px;
    height: 17px;
}

.send-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.chat-messages::-webkit-scrollbar {
    width: 5px;
}

.chat-messages::-webkit-scrollbar-thumb {
    background: #cdd6de;
    border-radius: 10px;
}

@media (max-width: 540px) {
    #chat-window {
        right: 10px;
        left: 10px;
        width: auto;
        bottom: 96px;
        height: 80vh;
    }

    #chat-toggle-btn {
        right: 18px;
        bottom: 18px;
    }
}
</style>