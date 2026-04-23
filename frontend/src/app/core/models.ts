export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  role: 'User' | 'Admin' | 'Technician';
}

export interface UserSession extends AuthResponse {}

export interface ChatAnalysis {
  issue: string;
  category: string;
  location?: string | null;
  priority: string;
  solution: string;
  intent: string;
  confidence: number;
  requiresHumanHandoff: boolean;
  handoffReason?: string | null;
  shouldOfferTicket: boolean;
  botMessage: string;
}

export interface Ticket {
  id: number;
  issue: string;
  category: string;
  location?: string | null;
  priority: string;
  status: string;
  createdAt: string;
  createdBy: number;
  createdByName?: string | null;
  assignedTo?: string | null;
  canCurrentUserUpdate: boolean;
}

export interface TicketComment {
  id: number;
  body: string;
  createdAt: string;
  createdBy: number;
  createdByName?: string | null;
  createdByRole?: string | null;
}

export interface UserSummary {
  id: number;
  name: string;
  email: string;
  role: 'User' | 'Admin' | 'Technician';
}

export interface KnowledgeBaseArticle {
  id: number;
  title: string;
  category: string;
  guidance: string;
  isActive: boolean;
}

export interface MessageBubble {
  sender: 'user' | 'bot';
  text: string;
}
