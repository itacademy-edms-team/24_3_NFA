import React from 'react';
import { FaUserCircle, FaPen } from 'react-icons/fa';

interface ProfileHeaderProps {
  nickname?: string;
  fio?: string;
  birthday?: string;
  status?: string;
  onEdit?: () => void;
}

const ProfileHeader: React.FC<ProfileHeaderProps> = ({
  nickname = 'Nickname',
  fio = 'FIO',
  birthday = 'Birthday date',
  status = 'Status',
  onEdit,
}) => {
  return (
    <div className="bg-white px-4 py-6 border-b border-[#E5E5EA]">
      <div className="flex items-start justify-between mb-4">
        <h1 className="text-[18px] font-semibold text-[#1A1A1A]">Your Profile</h1>
        {onEdit && (
          <button onClick={onEdit} className="p-2 text-[#8E8E93] hover:text-[#1A1A1A]">
            <FaPen className="w-4 h-4" />
          </button>
        )}
      </div>

      <div className="flex items-center gap-4">
        <FaUserCircle className="w-16 h-16 text-[#6B5B95]" />
        <div className="flex-1">
          <h2 className="text-[16px] font-semibold text-[#1A1A1A] mb-1">{nickname}</h2>
          <p className="text-[13px] text-[#6B6B6B]">{fio}</p>
          <p className="text-[12px] text-[#8E8E93]">{birthday}</p>
          <p className="text-[12px] text-[#8E8E93]">{status}</p>
        </div>
      </div>
    </div>
  );
};

export default ProfileHeader;
